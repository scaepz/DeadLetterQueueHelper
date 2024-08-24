using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class SelectedQueueState : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

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
            Console.WriteLine("Select queue");
            SelectedQueue = queue;
            using (Computed.Invalidate())
            {
                GetSelectedQueue();
            }
        }
    }
}
