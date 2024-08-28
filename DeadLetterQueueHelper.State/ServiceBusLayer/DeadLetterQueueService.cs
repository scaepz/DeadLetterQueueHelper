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

        [ComputeMethod(AutoInvalidationDelay = 30)]
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

        public async virtual Task<IReadOnlyList<ServiceBusReceivedMessage>> ForcePeekAllDeadLetters()
        {
            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters();
            }
            return await PeekAllDeadLetters();
        }

        public async Task DeleteDeadLetters(string messageId)
        {
            var deadLetterQueue = await _clientProvider.GetReceiver(SubQueue.DeadLetter);
            if (deadLetterQueue == null)
            {
                throw new InvalidOperationException($"Couldn't get a DLQ receiver when I tried to delete {messageId}.");
            }

            var messages = await deadLetterQueue.ReceiveMessagesAsync(1000);

            foreach (var message in messages)
            {
                Console.WriteLine("message: " + message);
                if (message.MessageId == messageId)
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


        public async Task Send(ServiceBusReceivedMessage originalMessage)
        {
            var sender = await _clientProvider.GetSender();
            if (sender == null)
            {
                throw new InvalidOperationException("Could not get a service bus sender");
            }

            var messageToSend = new ServiceBusMessage(originalMessage);

            await sender.SendMessageAsync(messageToSend);
        }
    }
}
