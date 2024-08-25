using DeadLetterQueueHelper.State.ServiceBusLayer;
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
                    var attempts = groupedDeadLetters
                        .Select(deadLetter => new IntegrationMessageAttempt(deadLetter.SequenceNumber, deadLetter.Body.ToString()))
                        .ToList();

                    return new IntegrationMessage(groupedDeadLetters.Key, attempts);
                })
                .ToList();
        }
    }
}
