using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class TimeService : IComputeService
    {
        private readonly TimeProvider _timeProvider;

        public TimeService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        [ComputeMethod]
        public Task<DateTimeOffset> GetTime()
        {
            return Task.FromResult(_timeProvider.GetLocalNow());
        }

        public void Refresh()
        {
            using (Computed.Invalidate())
            {
                _ = GetTime();
            }
        }
    }
}
