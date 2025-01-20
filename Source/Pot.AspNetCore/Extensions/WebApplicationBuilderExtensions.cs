using AllOverIt.Validation.Extensions;
using Pot.AspNetCore.ExceptionHandlers;
using Pot.AspNetCore.Logging;
using Pot.AspNetCore.Middleware;
using Pot.AspNetCore.Validation;
using Pot.Data;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCorrelationId(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CorrelationIdMiddleware>();

        return builder;
    }

    public static WebApplicationBuilder AddOpenApi(this WebApplicationBuilder builder)
    {
        // Refer to this link if multiple versions are required and different pages should be shown:
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi
        builder.Services.AddOpenApi("v1", options => { });

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
            .AddExceptionHandler<EntityExceptionHandler>()
            .AddExceptionHandler<DatabaseExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
    {
        // IProblemDetailsService is registered as a singleton.
        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
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

        builder.Services.AddSingleton<IProblemDetailsInspector, ProblemDetailsInspector>();

        return builder;
    }

    public static WebApplicationBuilder AddPotData(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<PotDbContext>();
        builder.Services.AddUnitOfWork();

        return builder;
    }
}
