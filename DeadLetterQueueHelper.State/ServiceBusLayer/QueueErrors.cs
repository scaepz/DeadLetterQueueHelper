using DeadLetterQueueHelper.State.Models;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class QueueErrors : IComputeService
    {
        private readonly Dictionary<Queue, string> _queueErrors = new();

        [ComputeMethod]
        public virtual Task<string?> GetError(Queue queue)
        {
            if (_queueErrors.TryGetValue(queue, out var error))
                return Task.FromResult<string?>(error);
            else
                return Task.FromResult<string?>(null);
        }

        public void SetError(Queue queue, string errorMessage)
        {
            if (_queueErrors.ContainsKey(queue) && _queueErrors[queue] == errorMessage)
                return;

            _queueErrors[queue] = errorMessage;
            using (Computed.Invalidate())
            {
                _ = GetError(queue);
            }
        }

        public void ClearError(Queue queue)
        {
            if (_queueErrors.Remove(queue))
            {
                using (Computed.Invalidate())
                {
                    _ = GetError(queue);
                }
            }
        }
    }
}
