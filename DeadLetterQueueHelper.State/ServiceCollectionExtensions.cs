using Blazored.LocalStorage;
using DeadLetterQueueHelper.State.AppStateLayer;
using DeadLetterQueueHelper.State.IntegrationMessageLayer;
using DeadLetterQueueHelper.State.ServiceBusLayer;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeadLetterQueueHelper.State
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterStateServices(this IServiceCollection services)
        {
            var fusion = services.AddFusion();
            fusion.AddService<SelectedQueuesService>(ServiceLifetime.Scoped);
            fusion.AddService<ServiceBusClientProvider>(ServiceLifetime.Scoped);
            fusion.AddService<DeadLetterQueueService>(ServiceLifetime.Scoped);
            fusion.AddService<IntegrationMessageService>(ServiceLifetime.Scoped);
            fusion.AddService<QueueErrors>(ServiceLifetime.Scoped);

            services.AddScoped<AccessTokenCredential>();

            services.AddScoped<QueueMonitor>();
            services.AddSingleton(TimeProvider.System);

            services.AddBlazoredLocalStorage(config =>
            {
                config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                config.JsonSerializerOptions.WriteIndented = false;
            });

            return services;
        }
    }
}
