using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class SelectedQueueState : IComputeService
    {
        private string SelectedQueue { get; set; } = "";

        [ComputeMethod]
        public virtual Task<string?> GetSelectedQueue()
        {
            if (string.IsNullOrWhiteSpace(SelectedQueue))
            {
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(SelectedQueue);
        }

        public void SelectQueue(string queue)
        {
            SelectedQueue = queue;
            using (Computed.Invalidate())
            {
                GetSelectedQueue();
            }
        }
    }
}
