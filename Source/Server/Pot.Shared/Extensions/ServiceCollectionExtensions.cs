using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pot.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TType"/> as a singleton resolved directly from its <see cref="IOptions{TOptions}"/> binding,
    /// allowing <typeparamref name="TType"/> to be injected without wrapping it in <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TType">The options type to expose as a singleton.</typeparam>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <returns>The same <paramref name="services"/> instance so calls can be chained.</returns>
    public static IServiceCollection AddSingletonFromOptions<TType>(this IServiceCollection services)
        where TType : class
    {
        // Allow for injection of TType instead of IOptions<TType>
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<TType>>().Value);

        return services;
    }
}
