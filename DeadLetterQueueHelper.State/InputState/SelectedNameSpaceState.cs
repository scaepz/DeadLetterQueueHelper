using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class SelectedNameSpaceState : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

        private string SelectedNamespace { get; set; } = "";

        [ComputeMethod]
        public virtual Task<string?> GetSelectedNamespaceOrNull()
        {
            if (string.IsNullOrWhiteSpace(SelectedNamespace))
            {
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(SelectedNamespace);
        }

        public void SelectNamespace(string newNamespace)
        {
            SelectedNamespace = newNamespace;
            using (Computed.Invalidate())
            {
                GetSelectedNamespaceOrNull();
            }
        }
    }
}
