using Azure.Messaging.ServiceBus;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class ServiceBusClientProvider : IComputeService
    {
        private readonly SelectedNameSpaceState _selectedNameSpaceState;
        private readonly AccessTokenService _accessTokenService;

        public ServiceBusClientProvider(SelectedNameSpaceState selectedNameSpaceState, AccessTokenService accessTokenService)
        {
            _selectedNameSpaceState = selectedNameSpaceState;
            _accessTokenService = accessTokenService;
        }

        [ComputeMethod]
        public virtual ServiceBusClient? GetServiceBusClient()
        {
            var selectedNamespace = _selectedNameSpaceState.GetSelectedNamespaceOrNull();

            if (selectedNamespace == null)
            {
                return null;
            }

            var accessToken = _accessTokenService.GetAccessToken();
            if (accessToken == null)
            {
                return null;
            }

            return null;
        }
    }
}
