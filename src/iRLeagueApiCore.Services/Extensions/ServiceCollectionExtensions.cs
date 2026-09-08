namespace iRLeagueApiCore.Server.Extensions;

static public class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the specified service as scoped if the exact self-binding is not already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceType">The service type.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddScopedExact(this IServiceCollection services, Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);

        if (!services.Any(x => x.ServiceType == serviceType && x.ImplementationType == serviceType))
        {
            services.AddScoped(serviceType);
        }

        return services;
    }

    /// <summary>
    /// Adds the specified service as scoped if the exact service/implementation pair is not already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddScopedExact(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        if (!services.Any(x => x.ServiceType == serviceType && x.ImplementationType == implementationType))
        {
            services.AddScoped(serviceType, implementationType);
        }

        return services;
    }

    /// <summary>
    /// Adds the specified service as scoped if the exact service/factory pair is not already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationFactory">The implementation factory.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddScopedExact(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        if (!services.Any(x => x.ServiceType == serviceType && x.ImplementationFactory == implementationFactory))
        {
            services.AddScoped(serviceType, implementationFactory);
        }

        return services;
    }

    /// <summary>
    /// Adds the specified service as scoped if the exact self-binding is not already registered.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddScopedExact<TService>(this IServiceCollection services)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(x => x.ServiceType == typeof(TService) && x.ImplementationType == typeof(TService)))
        {
            services.AddScoped<TService>();
        }

        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in TService with an implementation type specified in TImplementation
    /// if the exact service/implementation pair is not already registered.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddScopedExact<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(x => x.ServiceType == typeof(TService) && x.ImplementationType == typeof(TImplementation)))
        {
            services.AddScoped<TService, TImplementation>();
        }

        return services;
    }

    /// <summary>
    /// Adds the specified service as scoped if the exact service/factory pair is not already registered.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="implementationFactory">The implementation factory.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddScopedExact<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        if (!services.Any(x => x.ServiceType == typeof(TService) && x.ImplementationFactory == implementationFactory))
        {
            services.AddScoped<TService>(implementationFactory);
        }

        return services;
    }
}
