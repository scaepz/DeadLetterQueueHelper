using DeadLetterQueueHelper.State.AppStateLayer;
using DeadLetterQueueHelper.State.IntegrationMessageLayer;
using DeadLetterQueueHelper.State.ServiceBusLayer;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterStateServices(this IServiceCollection services)
        {
            var fusion = services.AddFusion();
            fusion.AddService<TimeService>(ServiceLifetime.Scoped);
            fusion.AddService<SelectedNameSpaceState>(ServiceLifetime.Scoped);
            fusion.AddService<SelectedQueueState>(ServiceLifetime.Scoped);
            fusion.AddService<ServiceBusClientProvider>(ServiceLifetime.Scoped);
            fusion.AddService<DeadLetterQueueService>(ServiceLifetime.Scoped);
            fusion.AddService<IntegrationMessageService>(ServiceLifetime.Scoped);

            services.AddScoped<AccessTokenCredential>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
