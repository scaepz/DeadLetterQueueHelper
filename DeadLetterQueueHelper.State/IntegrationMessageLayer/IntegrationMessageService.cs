using Blazored.LocalStorage;
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

        public IntegrationMessageService(DeadLetterQueueService deadLetterQueueService, ILocalStorageService localStorage, QueueMonitor queueMonitor)
        {
            _deadLetterQueueService = deadLetterQueueService;
            _localStorage = localStorage;
            _queueMonitor = queueMonitor;
        }

        public bool IsDisposed => false;


        [ComputeMethod]
        public async virtual Task<IReadOnlyList<IntegrationMessage>> GetFailedMessages()
        {
            var deadLetters = await _deadLetterQueueService.PeekAllDeadLetters();

            return deadLetters
                .GroupBy(x => x.MessageId)
                .Select(groupedDeadLetters =>
                {
                    return new IntegrationMessage(groupedDeadLetters.ToList());
                })
                .OrderBy(x => x.Attempts.First().EnqueuedTime)
                .ToList();
        }

        [ComputeMethod]
        public async virtual Task<bool> HasPendingResubmission(string messageId)
        {
            var storageName = GetPendingResubmissionStorageName(messageId);
            var pendingResubmissions = await _localStorage.GetItemAsync<List<string>>(storageName) ?? [];
            return pendingResubmissions.Contains(messageId);
        }

        public async virtual Task Resubmit(IntegrationMessage message)
        {
            if (await HasPendingResubmission(message.Id))
            {
                return;
            }
            await AddPendingResubmission(message.Id);

            long lastSequenceNumber = await GetLastSequenceNumber(message);

            await _deadLetterQueueService.Send(message.Attempts.First());

            _queueMonitor.CallbackWhenMessageDisappeared(new MonitorEntry
            (
                message.Id,
                lastSequenceNumber,
                HandleResubmissionDisappearedFromQueue)
            );
        }

        private async Task<long> GetLastSequenceNumber(IntegrationMessage message)
        {
            var deadLetters = await _deadLetterQueueService.ForcePeekAllDeadLetters();
            var lastSequenceNumber = deadLetters
                .Where(x => x.MessageId == message.Id)
                .Select(x => x.SequenceNumber)
                .Last();
            return lastSequenceNumber;
        }

        private async Task HandleResubmissionDisappearedFromQueue(MonitorEntry disappearedMessage)
        {
            var deadLetters = await _deadLetterQueueService.ForcePeekAllDeadLetters();
            var newlyDeadLettered = deadLetters.FirstOrDefault(x => x.SequenceNumber > disappearedMessage.PreviousSequenceNumber && x.MessageId == disappearedMessage.MessageId);

            await RemovePendingResubmission(disappearedMessage.MessageId);

            if (newlyDeadLettered != null) 
                return;

            await _deadLetterQueueService.DeleteDeadLetters(disappearedMessage.MessageId);
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
