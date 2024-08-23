using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class SelectedNameSpaceState : IComputeService
    {
        private string SelectedNamespace { get; set; } = "";

        [ComputeMethod]
        public virtual string? GetSelectedNamespaceOrNull()
        {
            if (string.IsNullOrWhiteSpace(SelectedNamespace))
            {
                return null;
            }

            return SelectedNamespace;
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
