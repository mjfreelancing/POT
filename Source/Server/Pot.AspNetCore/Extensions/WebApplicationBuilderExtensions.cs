using AllOverIt.DependencyInjection.Extensions;
using AllOverIt.Serialization.Json.SystemText.Converters;
using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
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
using Pot.AspNetCore.Concerns.OpenApi;
using Pot.AspNetCore.Concerns.ProblemDetails;
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
    // The Problem Details member carrying the support id. ProblemDetailsSchemaTransformer documents it by this name.
    internal const string CorrelationIdExtension = "correlationId";

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
                        var migrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);

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
        builder.Services.AddScoped<SecurityHeadersMiddleware>();
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
            options.CreateSchemaReferenceId = SchemaReferenceId.Create;

            // Without this the title is the assembly name and document name, "Pot.AspNetCore | v1" - the UI's page
            // heading, and what some generators name the client class after.
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "POT API";

                document.Info.Description =
                    "Every error response is Problem Details (RFC 9457), carrying a `correlationId` to quote when " +
                    $"reporting a problem. The same id is returned on every response in the `{CorrelationIdMiddleware.HeaderName}` header.";

                return Task.CompletedTask;
            });

            options.AddSchemaTransformer<ProblemDetailsSchemaTransformer>();

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

            // CorrelationIdMiddleware accepts the header on every request and echoes it on every response without any
            // endpoint declaring either, so nothing else puts it in the document. Optional on the request, and
            // constrained exactly as the middleware enforces it, so a generated client neither demands it nor sends a
            // value that is then rejected with a 400.
            options.AddOperationTransformer((operation, _, _) =>
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = CorrelationIdMiddleware.HeaderName,
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "An id of the caller's own to identify this request by in the logs and in any " +
                        "error response. Optional: where it is absent the API generates one. An id that is too long " +
                        "or uses another character is rejected with 400, never altered.",
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        MaxLength = CorrelationIdMiddleware.MaxCorrelationIdLength,
                        Pattern = CorrelationIdMiddleware.AllowedCharactersPattern
                    }
                });

                foreach (var response in (operation.Responses ?? []).Values.OfType<OpenApiResponse>())
                {
                    response.Headers ??= new Dictionary<string, IOpenApiHeader>(StringComparer.OrdinalIgnoreCase);
                    response.Headers[CorrelationIdMiddleware.HeaderName] = new OpenApiHeader
                    {
                        Required = true,
                        Description = "Identifies this request in the logs - the caller's own id where one was " +
                            "accepted, otherwise one the API generated. Quote it when reporting a problem.",
                        Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                    };
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

        // One request-log entry per request, on fields chosen rather than inherited. No request body and no headers are
        // logged in any environment: a body can hold a password or an OTP, and header values are worse. CombineLogs
        // produces one entry when the response completes, so a failure is recorded with the status the caller received.
        // The narrower category entry in appsettings.json is what lets these entries through "Microsoft.AspNetCore":
        // "Warning"; deleting it turns request logging off without any other sign that it has gone.
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod |
                HttpLoggingFields.RequestPath |
                HttpLoggingFields.RequestQuery |
                HttpLoggingFields.ResponseStatusCode |
                HttpLoggingFields.Duration;

            options.CombineLogs = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddExceptionHandlers(this WebApplicationBuilder builder)
    {
        // Exception handlers are registered as a singleton.
        // Ordering of registered exception handlers matter.
        // There is no handler for an aborted request: the exception handler middleware recognises a client
        // disconnect (OperationCanceledException or IOException with RequestAborted cancelled), records 499
        // and returns before calling any IExceptionHandler.
        // In Development the framework throws BadHttpRequestException when it cannot read a request.
        // Without a handler for it the exception handler middleware returns 500. The handler returns the
        // status the framework put on the exception instead - 400 for a binding failure, 413 for a body
        // past the size limit - so Development behaves like every other environment.
        builder.Services
            .AddExceptionHandler<BadRequestExceptionHandler>()
            .AddExceptionHandler<DatabaseExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddProblemDetails(this WebApplicationBuilder builder)
    {
        // IProblemDetailsService is registered as a singleton.
        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions[CorrelationIdExtension] = ctx.HttpContext.TraceIdentifier;
                ctx.ProblemDetails.Instance = ctx.HttpContext.Request.Path.ToString();
            });

        // Wrap the framework's writer so a request whose Accept header excludes JSON still gets Problem Details - see
        // UnconditionalProblemDetailsWriter for how each route fails otherwise. Found by its service type because the
        // framework's implementation is internal and cannot be named.
        var writers = builder.Services
            .Where(descriptor => descriptor.ServiceType == typeof(IProblemDetailsWriter))
            .ToList();

        var frameworkWriter = writers.Count == 1 ? writers[0] : null;
        var frameworkWriterType = frameworkWriter?.ImplementationType;

        // Exactly one writer, registered by type, is what AddProblemDetails() produces today. Anything else - MVC adding
        // a writer of its own, or the framework registering this one by factory - would leave another writer still
        // declining non-JSON requests, so startup fails and says why instead.
        if (frameworkWriter is null || frameworkWriterType is null)
        {
            throw new InvalidOperationException(
                $"Expected exactly one {nameof(IProblemDetailsWriter)}, registered by type by AddProblemDetails(), " +
                $"to wrap in {nameof(UnconditionalProblemDetailsWriter)}. Found {writers.Count}.");
        }

        builder.Services.Remove(frameworkWriter);

        // The framework writer's type is internal and no longer registered, so it is built from its type, resolving its
        // constructor's dependencies from the container when the writer is first resolved.
        UnconditionalProblemDetailsWriter CreateWrappedWriter(IServiceProvider serviceProvider)
        {
            var innerWriter = (IProblemDetailsWriter)ActivatorUtilities.CreateInstance(serviceProvider, frameworkWriterType);

            return new UnconditionalProblemDetailsWriter(innerWriter);
        }

        // The framework's own lifetime, so wrapping changes whether it writes and nothing else.
        builder.Services.Add(ServiceDescriptor.Describe(typeof(IProblemDetailsWriter), CreateWrappedWriter, frameworkWriter.Lifetime));

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
