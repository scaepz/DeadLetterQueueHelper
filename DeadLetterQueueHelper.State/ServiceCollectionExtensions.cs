using DeadLetterQueueHelper.State.AccessTokens;
using DeadLetterQueueHelper.State.DeadLetterQueueServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.AddScoped<AccessTokenCredential>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
