using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.ExceptionHandlers;
using Pot.AspNetCore.Logging;
using Pot.AspNetCore.Middleware;
using Pot.Data;

namespace Pot.AspNetCore.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCorrelationId(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CorrelationIdMiddleware>();

        return builder;
    }

    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        var loggingBuilder = builder.Logging;

        builder.Services
            .AddHttpContextAccessor()                       // CorrelationIdLogEnricher requires IHttpContextAccessor
            .AddLogEnricher<CorrelationIdLogEnricher>();    // Requires Microsoft.Extensions.Telemetry

        loggingBuilder
            .ClearProviders()
            .EnableEnrichment(/*options => { }*/)
            .AddConsole(options =>
            {
                options.FormatterName = PotConsoleFormatter.FormatterName;
            })
            .AddConsoleFormatter<PotConsoleFormatter, PotConsoleFormatterOptions>(options =>
            {
                options.IncludeScopes = false;
                options.TimestampFormat = "HH:mm:ss";
                options.UseUtcTimestamp = true;
                options.SingleLine = true;
            }); ;

        //loggingBuilder.AddJsonConsole(options =>
        //{
        //    options.IncludeScopes = false;
        //    options.TimestampFormat = "HH:mm:ss";
        //    options.JsonWriterOptions = new JsonWriterOptions
        //    {
        //        Indented = true
        //    };
        //});

        //loggingBuilder.AddSimpleConsole(options =>
        //{
        //    options.IncludeScopes = true;
        //    options.SingleLine = true;
        //    options.TimestampFormat = "HH:mm:ss ";
        //    options.UseUtcTimestamp = true;
        //});

        return builder;
    }

    public static WebApplicationBuilder AddExceptionHandlers(this WebApplicationBuilder builder)
    {
        // Exception handlers are registered as a singleton.
        // Ordering of registered exception handlers matter.
        builder.Services
            .AddExceptionHandler<IgnoreExceptionHandler>()
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<EntityExceptionHandler>()
            .AddExceptionHandler<DatabaseExceptionHandler>()
            .AddExceptionHandler<UnhandledExceptionHandler>();          // This MUST be the last handler

        return builder;
    }

    public static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
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
