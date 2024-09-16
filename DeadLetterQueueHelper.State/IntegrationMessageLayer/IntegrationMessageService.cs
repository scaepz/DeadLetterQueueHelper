using Azure.Messaging.ServiceBus;
using Blazored.LocalStorage;
using DeadLetterQueueHelper.State.AppStateLayer;
using DeadLetterQueueHelper.State.ServiceBusLayer;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public class IntegrationMessageService : IComputeService, IHasIsDisposed
    {
        private readonly DeadLetterQueueService _deadLetterQueueService;
        private readonly ILocalStorageService _localStorage;
        private readonly QueueMonitor _queueMonitor;
        private readonly SelectedQueuesService _selectedQueuesService;

        public IntegrationMessageService(
            DeadLetterQueueService deadLetterQueueService,
            ILocalStorageService localStorage,
            QueueMonitor queueMonitor,
            SelectedQueuesService selectedQueuesService)
        {
            _deadLetterQueueService = deadLetterQueueService;
            _localStorage = localStorage;
            _queueMonitor = queueMonitor;
            _selectedQueuesService = selectedQueuesService;
        }

        public bool IsDisposed => false;


        [ComputeMethod]
        public async virtual Task<IReadOnlyList<IntegrationMessage>> GetFailedMessages()
        {
            var queues = await _selectedQueuesService.GetSelectedQueues();

            var integrationMessages = new List<IntegrationMessage>();

            foreach (var queue in queues)
            {
                var deadLetters = await _deadLetterQueueService.PeekAllDeadLetters(queue);

                var messagesFromThisQueue = deadLetters
                    .GroupBy(x => x.MessageId)
                    .Select(groupedDeadLetters =>
                    {
                        return new IntegrationMessage(queue, groupedDeadLetters.ToList());
                    });

                integrationMessages.AddRange(messagesFromThisQueue);
            }


            return integrationMessages
                .OrderBy(x => x.FirstEnqueuedTime)
                .ToList();
        }

        [ComputeMethod]
        public async virtual Task<bool> HasPendingResubmission(string messageId)
        {
            var storageName = GetPendingResubmissionStorageName(messageId);
            var pendingResubmissions = await _localStorage.GetItemAsync<List<string>>(storageName) ?? [];
            return pendingResubmissions.Contains(messageId);
        }

        public async virtual Task Resubmit(IntegrationMessage message, EditableAttemptProperties? withValues = null)
        {
            if (await HasPendingResubmission(message.Id))
            {
                return;
            }
            await AddPendingResubmission(message.Id);

            long lastSequenceNumber = await GetLastSequenceNumber(message);

            await _deadLetterQueueService.Send(message.Queue, message.Attempts.First(), withValues);

            _queueMonitor.CallbackWhenMessageDisappeared(new MonitorEntry
            (
                message.Queue,
                message.Id,
                lastSequenceNumber,
                HandleResubmissionDisappearedFromQueue)
            );
        }

        private async Task<long> GetLastSequenceNumber(IntegrationMessage message)
        {
            var deadLetters = await _deadLetterQueueService.ForcePeekAllDeadLetters(message.Queue);
            var lastSequenceNumber = deadLetters
                .Where(x => x.MessageId == message.Id)
                .Select(x => x.SequenceNumber)
                .Last();
            return lastSequenceNumber;
        }

        private async Task HandleResubmissionDisappearedFromQueue(MonitorEntry disappearedMessage)
        {
            var deadLetters = await _deadLetterQueueService.ForcePeekAllDeadLetters(disappearedMessage.Queue);
            var newlyDeadLettered = deadLetters.FirstOrDefault(x => x.SequenceNumber > disappearedMessage.PreviousSequenceNumber && x.MessageId == disappearedMessage.MessageId);

            await RemovePendingResubmission(disappearedMessage.MessageId);

            if (newlyDeadLettered != null)
                return;

            await _deadLetterQueueService.DeleteDeadLetters(disappearedMessage.Queue, disappearedMessage.MessageId);
        }

        private async Task AddPendingResubmission(string messageId)
        {
            var storageName = GetPendingResubmissionStorageName(messageId);

            var resubmissions = await _localStorage.GetItemAsync<List<string>>(storageName) ?? [];

            if (resubmissions.Contains(messageId))
                return;

            resubmissions.Add(messageId);

            await _localStorage.SetItemAsync(storageName, resubmissions);

            using (Computed.Invalidate())
            {
                _ = HasPendingResubmission(messageId);
            }
        }

        private async Task RemovePendingResubmission(string messageId)
        {
            var storageName = GetPendingResubmissionStorageName(messageId);

            var resubmissions = await _localStorage.GetItemAsync<List<string>>(storageName) ?? [];

            if (!resubmissions.Contains(messageId))
                return;

            resubmissions.Remove(messageId);

            await _localStorage.SetItemAsync(storageName, resubmissions);

            using (Computed.Invalidate())
            {
                _ = HasPendingResubmission(messageId);
            }
        }

        private string GetPendingResubmissionStorageName(string messageId)
        {
            return $"pendingResubmissions";
        }
    }
}
