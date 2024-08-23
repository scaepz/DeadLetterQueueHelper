using DeadLetterQueueHelper.State.AccessTokens;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterStateServices(this IServiceCollection services)
        {
            var fusion = services.AddFusion();
            fusion.AddService<TimeService>();
            fusion.AddService<SelectedNameSpaceState>();
            fusion.AddService<ServiceBusClientProvider>();
            
            services.AddSingleton<AccessTokenCredential>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
