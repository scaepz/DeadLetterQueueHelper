using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class TimeService : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

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
