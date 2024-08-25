using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.AppStateLayer;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class DeadLetterQueueService : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

        private readonly ServiceBusClientProvider _clientProvider;
        private readonly SelectedQueueState _selectedQueueState;
        private readonly QueueMonitor _queueMonitor;

        public DeadLetterQueueService(ServiceBusClientProvider clientProvider, SelectedQueueState selectedQueueState, QueueMonitor queueMonitor)
        {
            _clientProvider = clientProvider;
            _selectedQueueState = selectedQueueState;
            _queueMonitor = queueMonitor;
        }

        [ComputeMethod(AutoInvalidationDelay = 15)]
        public async virtual Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekAllDeadLetters()
        {
            Console.WriteLine("PeekAllDeadLetters");
            var receiver = await _clientProvider.GetReceiver(SubQueue.DeadLetter);

            if (receiver == null)
            {
                Console.WriteLine("Receiver was null");
                return new List<ServiceBusReceivedMessage>();
            }

            return await receiver.PeekMessagesAsync(1000, fromSequenceNumber: 0);
        }

        [ComputeMethod]
        public async virtual Task<int?> GetDeadLetterCount()
        {
            Console.WriteLine("GetDeadLetterCount");

            var messages = await PeekAllDeadLetters();

            return messages.Count;
        }


        public async Task Resubmit(ServiceBusReceivedMessage originalMessage)
        {
            var sender = await _clientProvider.GetSender();
            if (sender == null)
            {
                throw new InvalidOperationException("Could not get a service bus sender");
            }

            var messageToSend = new ServiceBusMessage(originalMessage);

            var deadLetters = await PeekAllDeadLetters();
            var lastSequenceNumber = deadLetters
                .Where(x => x.MessageId == originalMessage.MessageId)
                .Select(x => x.SequenceNumber)
                .Max();

            await sender.SendMessageAsync(messageToSend);

            _queueMonitor.CallbackWhenMessageDisappeared(new MonitorEntry
            (
                messageToSend.MessageId,
                lastSequenceNumber,
                HandleResubmissionDisappearedFromQueue)
            );

            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters();
            }
        }

        private async Task HandleResubmissionDisappearedFromQueue(MonitorEntry disappearedMessage)
        {
            Console.WriteLine("HandleResubmissionDisappearedFromQueue");
            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters();
            }

            var deadLetters = await PeekAllDeadLetters();
            var newlyDeadLettered = deadLetters.FirstOrDefault(x => x.SequenceNumber > disappearedMessage.PreviousSequenceNumber && x.MessageId == disappearedMessage.MessageId);

            Console.WriteLine("newlyDeadLettered" + newlyDeadLettered);

            if (newlyDeadLettered != null)
                return;

            var deadLetterQueue = await _clientProvider.GetReceiver(SubQueue.DeadLetter);
            if (deadLetterQueue == null)
            {
                throw new InvalidOperationException($"Couldn't get a DLQ receiver at a crucial moment. The resubmitted message with id {disappearedMessage.MessageId} succeeded, but we couldn't clean it up from the DLQ.");
            }

            var messages = await deadLetterQueue.ReceiveMessagesAsync(1000);

            foreach (var message in messages)
            {
                Console.WriteLine("message: " + message);
                if (message.MessageId == disappearedMessage.MessageId)
                {

                    await deadLetterQueue.CompleteMessageAsync(message);
                }
                else
                {
                    await deadLetterQueue.AbandonMessageAsync(message);
                }
            }

            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters();
            }
        }
    }
}
