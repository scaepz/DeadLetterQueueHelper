using Blazored.LocalStorage;
using DeadLetterQueueHelper.State.Models;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.AppStateLayer
{
    public class SelectedQueuesService : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

        private readonly ILocalStorageService _storage;
        private const string StorageName = "queues";

        public SelectedQueuesService(ILocalStorageService storage)
        {
            _storage = storage;
        }

        public async Task Add(Queue queue)
        {
            var queues = await GetAllQueues();
            if (!queues.Contains(queue))
            {
                queues.Add(queue);
                await _storage.SetItemAsync(StorageName, queues);
            }

            using (Computed.Invalidate())
            {
                _ = GetAllQueues();
            }
        }

        public async Task ToggleSelection(Queue queue)
        {
            var queues = await GetAllQueues();

            var index = queues.FindIndex(x => x.QueueName == queue.QueueName && x.Namespace == queue.Namespace);
            if (index == -1)
            {
                return;
            }

            queues[index] = queue with
            {
                IsSelected = !queue.IsSelected
            };

            await _storage.SetItemAsync(StorageName, queues);
            using (Computed.Invalidate())
            {
                _ = GetAllQueues();
            }
        }

        public async Task Remove(Queue queue)
        {
            var queues = await GetSelectedQueues();

            queues.Remove(queue);
            await _storage.SetItemAsync(StorageName, queues);

            using (Computed.Invalidate())
            {
                _ = GetAllQueues();
            }
        }

        [ComputeMethod]
        public virtual async Task<List<Queue>> GetSelectedQueues()
        {
            var queues = await GetAllQueues();

            return queues
                .Where(x => x.IsSelected)
                .ToList();
        }

        [ComputeMethod]
        public virtual async Task<List<Queue>> GetAllQueues()
        {
            return await _storage.GetItemAsync<List<Queue>>(StorageName) ?? [];
        }
    }
}
