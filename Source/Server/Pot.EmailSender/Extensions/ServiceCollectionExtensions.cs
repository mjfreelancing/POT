using AllOverIt.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pot.RazorComponents.Extensions;
using Pot.Shared.DependencyInjection;

namespace Pot.EmailSender.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailSending(this IServiceCollection services)
    {
        services.AutoRegisterScoped<DependencyRegistrar, IPotScopedDependency>(config =>
        {
            // Exclude interfaces we know we don't want to register
            config.Filter((serviceType, implementationType) =>
            {
                return serviceType != typeof(IPotScopedDependency);
            });
        });

        // IRazorComponentRenderer
        services.AddRazorComponentRendering();

        services.AddSingleton<ISendEmailChannelReader, SendEmailChannel>();

        services.AddSingleton<ISendEmailChannelWriter>(provider =>
        {
            var channel = provider.GetRequiredService<ISendEmailChannelReader>() as SendEmailChannel;
            return channel!;
        });

        return services;
    }
}
