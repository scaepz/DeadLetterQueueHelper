﻿using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.AccessTokens;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class ServiceBusClientProvider : IComputeService
    {
        private readonly SelectedNameSpaceState _selectedNameSpaceState;
        private readonly AccessTokenCredential _accessTokenCredential;

        public ServiceBusClientProvider(SelectedNameSpaceState selectedNameSpaceState, AccessTokenCredential accessTokenCredential)
        {
            _selectedNameSpaceState = selectedNameSpaceState;
            _accessTokenCredential = accessTokenCredential;
        }

        [ComputeMethod]
        public virtual ServiceBusClient? GetServiceBusClient()
        {
            var selectedNamespace = _selectedNameSpaceState.GetSelectedNamespaceOrNull();

            if (selectedNamespace == null)
            {
                return null;
            }

            return new ServiceBusClient(selectedNamespace, _accessTokenCredential);
        }
    }
}
