using AllOverIt.DependencyInjection.Extensions;
using AllOverIt.Serialization.Json.SystemText.Converters;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
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
    private const string BearerSecuritySchemeName = "Bearer";
    private static readonly Type ScopedLifetimeValidatorType = typeof(IScopedLifetimeValidator);

    public static WebApplicationBuilder AddPotHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<PotDbContext>(
                "database",
                tags: ["ready"],
                customTestQuery: async (context, cancellationToken) =>
                {
                    try
                    {
                        var migrations = await context.Database
                            .GetAppliedMigrationsAsync(cancellationToken)
                            .ConfigureAwait(false);

                        return migrations.Any();
                    }
                    catch
                    {
                        return false;
                    }
                });

        return builder;
    }

    public static WebApplicationBuilder AddPotAuth(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        ConfigureJwtOptions(services);
        ConfigureAuthenticationOptions(services);
        ConfigurePlatformAdminOptions(services);
        ConfigureAuthorization(services);

        services
            .AddOtpCleanup()
            .AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>()
            .AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return builder;
    }

    private static void ConfigureJwtOptions(IServiceCollection services)
    {
        // Binds configuration from the "Jwt" section onto a JwtOptions instance, which is later injected into JwtBearerOptionsSetup.
        services.ConfigureOptions<JwtOptionsSetup>();
        services.AddOptions<JwtOptions>().ValidateOnStart();

        services
            // Sets up Jwt ****** options - alternative approach to setting with AddJwtBearer().
            .ConfigureOptions<JwtBearerOptionsSetup>()
            .AddSingletonFromOptions<JwtOptions>()                  // Allow for injection of JwtOptions instead of IOptions<JwtOptions>

            // Configures JWT ****** event handlers to validate tokens against database state
            .ConfigureOptions<JwtBearerEventsSetup>();
    }

    private static void ConfigureAuthenticationOptions(IServiceCollection services)
    {
        // Bind AuthenticationOptions for features such as refresh token cookies.
        services.ConfigureOptions<AuthenticationOptionsSetup>();
        services.AddOptions<AuthenticationOptions>().ValidateOnStart();
        services.AddSingletonFromOptions<AuthenticationOptions>();   // Allow for injection of AuthenticationOptions instead of IOptions<AuthenticationOptions>
    }

    private static void ConfigurePlatformAdminOptions(IServiceCollection services)
    {
        // Binds configuration from the "PlatformAdmin" section onto a PlatformAdminOptions instance.
        services.ConfigureOptions<PlatformAdminOptionsSetup>();
        services.AddOptions<PlatformAdminOptions>().ValidateOnStart();
        services.AddSingletonFromOptions<PlatformAdminOptions>();    // Allow for injection of PlatformAdminOptions instead of IOptions<PlatformAdminOptions>
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
            {
                // Required for /me endpoint where the user needs to be authenticated, but no specific permissions are required.
                options.AddPolicy(AuthorizationPolicies.AuthenticatedUser, policy => policy.RequireAuthenticatedUser());

                // Fail closed: an endpoint that declares no requirement of its own is authenticated-only rather
                // than reachable anonymously, so a forgotten .RequireAuthorization(...) cannot ship an open route.
                // Endpoints that must stay reachable before a session exists opt out with .AllowAnonymous().
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            })
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Prevent the default mapping of claims. For example, A JwtRegisteredClaimNames.Sub claim would normally get mapped
                // to ClaimTypes.NameIdentifier (http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier).
                options.MapInboundClaims = false;
            });
    }

    public static WebApplicationBuilder AddPotCors(this WebApplicationBuilder builder)
    {
        builder.Services
            // Binds configuration from the "Cors" section onto a CorsConfiguration instance.
            .ConfigureOptions<CorsConfigurationSetup>();

        builder.Services
            .AddOptions<CorsConfiguration>()
            .ValidateOnStart();

        builder.Services
            // Allow for injection of CorsConfiguration instead of IOptions<CorsConfiguration>
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
        builder.Services.AddOpenApi("v1", options =>
        {
            // Feature slices reuse simple CLR names such as Request and Response.
            options.CreateSchemaReferenceId = jsonTypeInfo =>
            {
                const string featureNamespacePrefix = "Pot.AspNetCore.Features.";

                var type = jsonTypeInfo.Type;
                var namespaceName = type.Namespace;

                if (namespaceName is null || !namespaceName.StartsWith(featureNamespacePrefix, StringComparison.Ordinal))
                {
                    return OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
                }

                var featureName = namespaceName[featureNamespacePrefix.Length..].Replace('.', '_');

                return string.IsNullOrEmpty(featureName) ? type.Name : $"{featureName}_{type.Name}";
            };

            // Describe JWT bearer authentication in the document so API tooling knows how clients authenticate.
            // This documents the existing auth scheme; it does not enable or enforce authentication at runtime.
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerSecuritySchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                return Task.CompletedTask;
            });

            // Mark protected operations as requiring the bearer scheme, but leave explicitly anonymous
            // operations (such as sign-in and registration) usable without a token in API tooling.
            options.AddOperationTransformer((operation, context, _) =>
            {
                var allowsAnonymous = context.Description.ActionDescriptor.EndpointMetadata
                    .OfType<AllowAnonymousAttribute>()
                    .Any();

                if (!allowsAnonymous)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerSecuritySchemeName, context.Document)] = []
                    });
                }

                return Task.CompletedTask;
            });
        });

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
            .EnableEnrichment(/*options => { }*/);

        if (builder.Environment.IsDevelopment())
        {
            loggingBuilder
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
                });
        }
        else
        {
            // Container Apps stores console output in Log_s. JSON lets Log Analytics filter by
            // fields such as level and category instead of searching rendered message text:
            // ContainerAppConsoleLogs_CL
            // | where ContainerAppName_s == "pot-api"
            // | extend Event = parse_json(Log_s)
            // | where tostring(Event.LogLevel) == "Error"
            // | project TimeGenerated, tostring(Event.Category), tostring(Event.Message), Log_s
            loggingBuilder.AddJsonConsole(options =>
            {
                options.IncludeScopes = false;
                options.TimestampFormat = "O";
                options.UseUtcTimestamp = true;
            });
        }

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
                ctx.ProblemDetails.Extensions["correlationId"] = ctx.HttpContext.TraceIdentifier;
                ctx.ProblemDetails.Instance = ctx.HttpContext.Request.Path.ToString();
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
            .ConfigureOptions<SmtpConfigurationSetup>();

        builder.Services
            .AddOptions<SmtpConfiguration>()
            .ValidateOnStart();

        builder.Services
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
            });

        return builder;
    }
}
