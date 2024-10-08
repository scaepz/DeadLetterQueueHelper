﻿using Blazored.LocalStorage;
using DeadLetterQueueHelper.State.Models;
using DeadLetterQueueHelper.State.ServiceBusLayer;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.AppStateLayer
{
    public class SelectedQueuesService : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

        private readonly ILocalStorageService _storage;
        private const string _storageName = "queues";

        private readonly Dictionary<Queue, string> _queueErrors = new();

        public SelectedQueuesService(ILocalStorageService storage)
        {
            _storage = storage;
        }

        public async Task Add(Queue queue)
        {
            var queues = await GetSelectedQueues();
            if (!queues.Contains(queue))
            {
                queues.Add(queue);
                await _storage.SetItemAsync(_storageName, queues);
            }

            using (Computed.Invalidate())
            {
                _ = GetSelectedQueues();
            }
        }

        public async Task Remove(Queue queue)
        {
            var queues = await GetSelectedQueues();

            if (queues.Contains(queue))
            {
                queues.Remove(queue);
                await _storage.SetItemAsync(_storageName, queues);
            }

            using (Computed.Invalidate())
            {
                _ = GetSelectedQueues();
            }
        }

        [ComputeMethod]
        public virtual async Task<List<Queue>> GetSelectedQueues()
        {
            return await _storage.GetItemAsync<List<Queue>>(_storageName) ?? [];
        }
    }
}
