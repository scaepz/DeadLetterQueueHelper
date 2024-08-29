using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.AppStateLayer;
using DeadLetterQueueHelper.State.Models;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class ServiceBusClientProvider : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;
        private readonly AccessTokenCredential _accessTokenCredential;

        public ServiceBusClientProvider(AccessTokenCredential accessTokenCredential)
        {
            _accessTokenCredential = accessTokenCredential;
        }


        [ComputeMethod]
        public virtual Task<ServiceBusClient> GetServiceBusClient(string busNamespace)
        {
            return Task.FromResult(new ServiceBusClient(busNamespace, _accessTokenCredential, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets }));
        }

        [ComputeMethod]
        public async virtual Task<ServiceBusReceiver> GetReceiver(Queue queue, SubQueue subQueue)
        {
            var client = await GetServiceBusClient(queue.Namespace);

            return client.CreateReceiver(queue.QueueName, new ServiceBusReceiverOptions
            {
                SubQueue = subQueue,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            });
        }

        [ComputeMethod]
        public async virtual Task<ServiceBusSender> GetSender(Queue queue)
        {
            var client = await GetServiceBusClient(queue.Namespace);

            return client.CreateSender(queue.QueueName);
        }
    }
}
