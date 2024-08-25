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

        public DeadLetterQueueService(ServiceBusClientProvider clientProvider, SelectedQueueState selectedQueueState)
        {
            _clientProvider = clientProvider;
            _selectedQueueState = selectedQueueState;
        }

        [ComputeMethod(AutoInvalidationDelay = 15)]
        public async virtual Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekAllDeadLetters()
        {
            Console.WriteLine("PeekAllDeadLetters");
            var receiver = await GetReceiver();

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

        [ComputeMethod]
        protected async virtual Task<ServiceBusReceiver?> GetReceiver()
        {
            Console.WriteLine("GetReceiver");

            var client = await _clientProvider.GetServiceBusClient();

            if (client == null)
            {
                Console.WriteLine("Client was null");
                return null;
            }

            var queue = await _selectedQueueState.GetSelectedQueue();
            if (queue == null)
            {
                Console.WriteLine("Queue was null");

                return null;
            }

            return client.CreateReceiver(queue, new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            });
        }

        public async Task Resubmit(ServiceBusReceivedMessage message)
        {
            var sender = await GetSender();
            if (sender == null)
            {
                throw new InvalidOperationException("Could not get a service bus sender");
            }

            var messageToSend = new ServiceBusMessage(message);

            await sender.SendMessageAsync(messageToSend);
            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters();
            }
        }


        [ComputeMethod]
        protected async virtual Task<ServiceBusSender?> GetSender()
        {
            Console.WriteLine("GetSender");

            var client = await _clientProvider.GetServiceBusClient();

            if (client == null)
            {
                Console.WriteLine("Client was null");
                return null;
            }

            var queue = await _selectedQueueState.GetSelectedQueue();
            if (queue == null)
            {
                Console.WriteLine("Queue was null");

                return null;
            }

            return client.CreateSender(queue);
        }
    }
}
