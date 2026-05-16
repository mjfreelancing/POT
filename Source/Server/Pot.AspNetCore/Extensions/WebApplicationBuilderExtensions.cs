using AllOverIt.DependencyInjection.Extensions;
using AllOverIt.Serialization.Json.SystemText.Converters;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Pot.App.Concerns.Validation;
using Pot.App.Extensions;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.AspNetCore.Concerns.Converters.JsonSerialization;
using Pot.AspNetCore.Concerns.Cors.Configuration;
using Pot.AspNetCore.Concerns.Email.Configuration;
using Pot.AspNetCore.Concerns.ExceptionHandlers;
using Pot.AspNetCore.Concerns.Logging;
using Pot.AspNetCore.Concerns.Middleware;
using Pot.AspNetCore.Concerns.RateLimiting.Configuration;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Features.Auth.Extensions;
using Pot.Data;
using Pot.Data.Configuration;
using Pot.Data.Extensions;
using Pot.EmailSender.Configuration;
using Pot.EmailSender.Extensions;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
namespace Pot.AspNetCore.Extensions;

internal static class WebApplicationBuilderExtensions
{
    private static readonly Type ScopedLifetimeValidatorType = typeof(IScopedLifetimeValidator);

    public static WebApplicationBuilder AddPotAuth(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services
            // Binds configuration from the "Jwt" section onto a JwtOptions instance, which is later injected into JwtBearerOptionsSetup.
            .ConfigureOptions<JwtOptionsSetup>()

            // Sets up Jwt Bearer validation options - alternative approach to setting with AddJwtBearer().
            .ConfigureOptions<JwtBearerOptionsSetup>()
            .AddSingletonFromOptions<JwtOptions>()                  // Allow for injection of JwtOptions instead of IOptions<JwtOptions>

            // Configures JWT Bearer authentication event handlers to validate tokens against database state
            .ConfigureOptions<JwtBearerEventsSetup>()

            // Bind AuthenticationOptions for features such as refresh token cookies.
            .ConfigureOptions<AuthenticationOptionsSetup>()
            .AddSingletonFromOptions<AuthenticationOptions>()       // Allow for injection of AuthenticationOptions instead of IOptions<AuthenticationOptions>

            // Binds configuration from the "PlatformAdmin" section onto a PlatformAdminOptions instance.
            .ConfigureOptions<PlatformAdminOptionsSetup>()
            .AddSingletonFromOptions<PlatformAdminOptions>()        // Allow for injection of PlatformAdminOptions instead of IOptions<PlatformAdminOptions>

            .AddAuthorization(options =>
            {
                // Required for /me endpoint where the user needs to be authenticated, but no specific permissions are required.
                options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
            })
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Prevent the default mapping of claims. For example, A JwtRegisteredClaimNames.Sub claim would normally get mapped
                // to ClaimTypes.NameIdentifier (http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier).
                options.MapInboundClaims = false;
            });

        services
            .AddOtpCleanup()
            .AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>()
            .AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return builder;
    }

    public static WebApplicationBuilder AddPotCors(this WebApplicationBuilder builder)
    {
        builder.Services
            // Binds configuration from the "Cors" section onto a CorsConfiguration instance.
            .ConfigureOptions<CorsConfigurationSetup>()

            // Allow for injection of CorsConfiguration instead of IOptions<CorsConfigurationSetup>
            .AddSingletonFromOptions<CorsConfiguration>()

            // Sets up CORS policy options - applies the default policy configuration
            .ConfigureOptions<CorsOptionsSetup>()

            .AddCors();

        return builder;
    }

    public static WebApplicationBuilder AddPotRateLimiting(this WebApplicationBuilder builder)
    {
        // WARNING:
        //
        // RateLimiting configuration keys:
        //  - RateLimiting:Anonymous:PermitLimit
        //  - RateLimiting:Anonymous:WindowSeconds
        //  - RateLimiting:Authenticated:PermitLimit
        //  - RateLimiting:Authenticated:WindowSeconds
        //
        // must NOT be added to any configuration file (appsettings.json, appsettings.*.json, environment-specific overrides, etc.).
        // These are exclusively for E2E test runs, injected as command-line arguments by the Playwright webServer configuration.
        // The hardcoded defaults in RateLimiterDefaults are the production values and are the source of truth for production behaviour.
        builder.Services
            // Binds configuration from the "RateLimiting" section onto a RateLimitingConfiguration instance.
            // These options will only be present during E2E tests to allow for more aggressive rate limiting,
            // so that the rate limiting behaviour can be reliably tested without needing to wait for long windows.
            .ConfigureOptions<RateLimitingConfigurationSetup>()

            // Allow for injection of RateLimitingConfiguration instead of IOptions<RateLimitingConfiguration>
            .AddSingletonFromOptions<RateLimitingConfiguration>()

            // Sets up rate limiter options - configures the rejection handler and chained policy
            .ConfigureOptions<RateLimiterOptionsSetup>()

            .AddRateLimiter();

        return builder;
    }

    public static WebApplicationBuilder AddPotMiddleware(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RawRequestLoggingMiddleware>();  // Only used in a Production environment
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
            options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<UserStatus>.Create());
            options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<ApprovalStatus>.Create());
            options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<RenewalMode>.Create());
            options.SerializerOptions.Converters.Add(EnrichedEnumJsonConverter<AccrualPolicy>.Create());
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
            .AutoRegisterSingleton<DependencyRegistrar, IPotSingletonDependency>(config =>
            {
                // Exclude interfaces we know we don't want to register
                config.Filter((serviceType, implementationType) =>
                {
                    return serviceType != typeof(IPotSingletonDependency);
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
                return validatorType.IsAssignableTo(ScopedLifetimeValidatorType);
            });

            validationRegistry.AutoRegisterSingletonValidators<ValidationRegistrar>((modelType, validatorType) =>
            {
                // Validators are typically registered as singletons, so we look for the lack of IScopedLifetimeValidator.
                return !validatorType.IsAssignableTo(ScopedLifetimeValidatorType);
            });

            validationRegistry.AddAppValidators();
        });

        builder.Services.AddSingleton<IProblemDetailsInspector, ProblemDetailsInspector>();

        return builder;
    }

    public static WebApplicationBuilder AddSmtp(this WebApplicationBuilder builder)
    {
        builder.Services
            // Binds configuration from the "Smtp" section onto a SmtpConfiguration instance.
            .ConfigureOptions<SmtpConfigurationSetup>()

            // Allow for injection of SmtpConfiguration instead of IOptions<SmtpConfiguration>
            .AddSingletonFromOptions<SmtpConfiguration>()

            .AddEmailSending();

        return builder;
    }

    public static WebApplicationBuilder AddPotData(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDatabaseConfiguration()
            .AddDbContext<PotDbContext>((provider, options) =>
            {
                var databaseConfiguration = provider.GetRequiredService<DatabaseConfiguration>();
                var connectionString = databaseConfiguration.GetConnectionString();

                options.ConfigurePostgres(connectionString);
            })
            .AddUnitOfWork();

        return builder;
    }
}
