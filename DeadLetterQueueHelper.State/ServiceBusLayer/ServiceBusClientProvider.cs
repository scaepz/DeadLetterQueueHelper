using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.AppStateLayer;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class ServiceBusClientProvider : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;
        private readonly SelectedNameSpaceState _selectedNameSpaceState;
        private readonly SelectedQueueState _selectedQueueState;
        private readonly AccessTokenCredential _accessTokenCredential;

        public ServiceBusClientProvider(SelectedNameSpaceState selectedNameSpaceState, AccessTokenCredential accessTokenCredential, SelectedQueueState selectedQueueState)
        {
            _selectedNameSpaceState = selectedNameSpaceState;
            _accessTokenCredential = accessTokenCredential;
            _selectedQueueState = selectedQueueState;
        }


        [ComputeMethod]
        public async virtual Task<ServiceBusClient?> GetServiceBusClient()
        {
            var selectedNamespace = await _selectedNameSpaceState.GetSelectedNamespaceOrNull();

            if (selectedNamespace == null)
            {
                return null;
            }

            return new ServiceBusClient(selectedNamespace, _accessTokenCredential, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
        }

        [ComputeMethod]
        public async virtual Task<ServiceBusReceiver?> GetReceiver(SubQueue subQueue)
        {
            var client = await GetServiceBusClient();

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
                SubQueue = subQueue,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            });
        }

        [ComputeMethod]
        public async virtual Task<ServiceBusSender?> GetSender()
        {
            var client = await GetServiceBusClient();

            if (client == null)
            {
                return null;
            }

            var queue = await _selectedQueueState.GetSelectedQueue();
            if (queue == null)
            {
                return null;
            }

            return client.CreateSender(queue);
        }
    }
}
