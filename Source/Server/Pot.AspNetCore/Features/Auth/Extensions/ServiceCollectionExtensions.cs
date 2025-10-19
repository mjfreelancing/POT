using Pot.AspNetCore.Features.Auth.PasswordReset.Workers;

namespace Pot.AspNetCore.Features.Auth.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOtpCleanup(this IServiceCollection services)
    {
        services
            .AddHostedService<ExpiredOtpCleanupWorker>()
            .AddHostedService<SendEmailWorker>();

        return services;
    }
}

