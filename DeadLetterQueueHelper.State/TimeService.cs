using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public interface ITimeService : IComputeService
    {
        [ComputeMethod]
        Task<DateTimeOffset> GetTime();
    }

    public class TimeService : ITimeService
    {
        private readonly TimeProvider _timeProvider;

        public TimeService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        [ComputeMethod]
        public virtual Task<DateTimeOffset> GetTime()
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
