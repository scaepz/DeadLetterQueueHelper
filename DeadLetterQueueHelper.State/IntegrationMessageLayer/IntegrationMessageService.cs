using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.ServiceBusLayer;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public class IntegrationMessageService : IComputeService, IHasIsDisposed
    {
        private readonly DeadLetterQueueService _deadLetterQueueService;

        public IntegrationMessageService(DeadLetterQueueService deadLetterQueueService)
        {
            _deadLetterQueueService = deadLetterQueueService;
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
                .ToList();
        }

        public async virtual Task Resubmit(ServiceBusReceivedMessage message)
        {
            await _deadLetterQueueService.Resubmit(message);
        }
    }
}
