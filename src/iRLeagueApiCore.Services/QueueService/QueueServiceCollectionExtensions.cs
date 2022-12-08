using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class QueueServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundQueue(this IServiceCollection services, int queueCapacity = 100)
        {
            services.AddSingleton<IBackgroundTaskQueue>(ctx =>
            {
                return new BackgroundTaskQueue(queueCapacity);
            });
            services.AddHostedService<QueuedHostedService>();
            return services;
        }
    }
}
