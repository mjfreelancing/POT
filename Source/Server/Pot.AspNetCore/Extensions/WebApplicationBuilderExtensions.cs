using AllOverIt.DependencyInjection.Extensions;
using AllOverIt.Pagination.Extensions;
using AllOverIt.Serialization.Json.SystemText.Converters;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using Pot.App.Concerns.Validation;
using Pot.App.Extensions;
using Pot.AspNetCore.Concerns.Converters.JsonSerialization;
using Pot.AspNetCore.Concerns.DependencyInjection;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Pot.AspNetCore.Concerns.Logging;
using Pot.AspNetCore.Concerns.Middleware;
using Pot.AspNetCore.Concerns.Validation;
using Pot.Data;
using Pot.Data.Extensions;
using Pot.Shared;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Extensions;

internal static class WebApplicationBuilderExtensions
{
    private static readonly Type _scopedLifetimeValidatorType = typeof(IScopedLifetimeValidator);

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

    public static WebApplicationBuilder AddHttpJsonOptions(this WebApplicationBuilder builder)
    {
        // Required for request/response deserialization/serialization
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<Frequency>.Create());
            options.SerializerOptions.Converters.Add(new NullableGuidConverter());
        });

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
        // Note: BadHttpRequestException is returned as 500 during development, but 400 in production.
        builder.Services
            .AddExceptionHandler<IgnoreExceptionHandler>()
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

    public static WebApplicationBuilder AddAspNetDependencies(this WebApplicationBuilder builder)
    {
        builder.Services
            .AutoRegisterScoped<DependencyRegistrar, IPotScopedDependency>(config =>
            {
                // Exclude interfaces we know we don't want to register
                config.Filter((serviceType, implementationType) =>
                {
                    if (serviceType.IsGenericType)
                    {
                        var genericTypeDefinition = serviceType.GetGenericTypeDefinition();

                        // Not expecting other types, but only filter out those we expect
                        return !(genericTypeDefinition == typeof(IValidator<>) || genericTypeDefinition == typeof(ValidatorBase<>));
                    }

                    return serviceType != typeof(IPotScopedDependency);
                });
            })
            .AddAppDependencies();

        return builder;
    }

    public static WebApplicationBuilder AddAspNetValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddLifetimeValidationInvoker(validationRegistry =>
        {
            validationRegistry.AutoRegisterScopedValidators<ValidationRegistrar>((modelType, validatorType) =>
            {
                return validatorType.IsAssignableTo(_scopedLifetimeValidatorType);
            });

            validationRegistry.AutoRegisterSingletonValidators<ValidationRegistrar>((modelType, validatorType) =>
            {
                // Validators are typically registered as singletons, so we look for the lack of IScopedLifetimeValidator.
                return !validatorType.IsAssignableTo(_scopedLifetimeValidatorType);
            });

            validationRegistry.AddAppValidators();
        });

        builder.Services.AddSingleton<IProblemDetailsInspector, ProblemDetailsInspector>();

        return builder;
    }

    public static WebApplicationBuilder AddPotData(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<PotDbContext>();
        builder.Services.AddQueryPagination();
        builder.Services.AddUnitOfWork();

        return builder;
    }
}
