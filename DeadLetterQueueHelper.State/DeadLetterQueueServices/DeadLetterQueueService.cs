using Azure.Messaging.ServiceBus;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.DeadLetterQueueServices
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


        [ComputeMethod]
        public async virtual Task<int?> GetDeadLetterCount()
        {
            Console.WriteLine("GetDeadLetterCount");
            var receiver = await GetReceiver();

            if (receiver == null)
            {
                Console.WriteLine("Receiver was null");
                return null;
            }


            var messages = await receiver.PeekMessagesAsync(1000);

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
    }
}
