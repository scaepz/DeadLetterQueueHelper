using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
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

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
