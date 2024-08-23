using Azure.Messaging.ServiceBus;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.DeadLetterQueueServices
{
    public class DeadLetterQueueService : IComputeService
    {
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
            var receiver = await GetReceiver();
            if (receiver == null)
            {
                return null;
            }

            var messages = await receiver.PeekMessagesAsync(1000);

            return messages.Count;
        }

        [ComputeMethod]
        protected async virtual Task<ServiceBusReceiver?> GetReceiver()
        {
            var client = await _clientProvider.GetServiceBusClient();

            if (client == null)
            {
                return null;
            }

            var queue = await _selectedQueueState.GetSelectedQueue();
            if (queue == null)
            {
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
