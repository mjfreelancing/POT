using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.ExceptionHandlers;
using Pot.AspNetCore.Middleware;
using Pot.Data;

namespace Pot.AspNetCore.Extensions;

public static class WebApplicationBuilderExtensions
{
    internal static WebApplicationBuilder AddCorrelationId(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CorrelationIdMiddleware>();

        return builder;
    }

    internal static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        // Alternative to the default SimpleConsole Logger
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
        //
        //builder.Logging.AddJsonConsole(options =>
        //{
        //    options.IncludeScopes = false;
        //    options.TimestampFormat = "HH:mm:ss";
        //    options.JsonWriterOptions = new JsonWriterOptions
        //    {
        //        Indented = true
        //    };
        //});

        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
            options.UseUtcTimestamp = true;
        });

        return builder;
    }

    internal static WebApplicationBuilder AddExceptionHandlers(this WebApplicationBuilder builder)
    {
        // Exception handlers are registered as a singleton
        // Ordering of registered exception handlers matter
        builder.Services
            .AddExceptionHandler<IgnoreExceptionHandler>()
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<EntityExceptionHandler>()
            .AddExceptionHandler<DatabaseExceptionHandler>()
            .AddExceptionHandler<UnhandledExceptionHandler>();          // This MUST be the last handler

        return builder;
    }

    internal static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
    {
        // IProblemDetailsService is registered as a singleton.
        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
                // A 'traceId' is already added - it will be the same as the 'correlationId' if the first request in a chain of service-to-service calls.
                ctx.ProblemDetails.Extensions.Add("correlationId", ctx.HttpContext.TraceIdentifier);
                ctx.ProblemDetails.Extensions.Add("instance", $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}");
            });

        return builder;
    }

    public static WebApplicationBuilder AddValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddLifetimeValidationInvoker(validationRegistry =>
        {
            validationRegistry.AutoRegisterSingletonValidators<PotValidationRegistrar>();
        });

        return builder;
    }

    public static WebApplicationBuilder AddPotDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<PotDbContext>();

        return builder;
    }
}
