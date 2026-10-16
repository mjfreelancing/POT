using AllOverIt.Extensions;
using Microsoft.Extensions.Options;
using Pot.EmailSender.Configuration;

namespace Pot.AspNetCore.Concerns.Email.Configuration;

/// <summary>Configures and validates SMTP settings from application configuration.</summary>
public sealed class SmtpConfigurationSetup : IConfigureOptions<SmtpConfiguration>, IValidateOptions<SmtpConfiguration>
{
    private const string SectionName = "Smtp";

    private readonly IConfiguration _configuration;

    /// <summary>Creates a configuration binder and validator for SMTP settings.</summary>
    /// <param name="configuration">The application configuration source.</param>
    public SmtpConfigurationSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Applies configured values to the supplied options instance.</summary>
    /// <param name="options">The options instance to configure or validate.</param>
    public void Configure(SmtpConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

    /// <summary>Validates the supplied value and returns the validation result.</summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to configure or validate.</param>
    public ValidateOptionsResult Validate(string? name, SmtpConfiguration options)
    {
        if (options.Host.IsNullOrEmpty())
        {
            return InvalidOption(nameof(SmtpConfiguration.Host));
        }

        if (options.Authentication.Username.IsNullOrEmpty())
        {
            return InvalidOption($"{nameof(SmtpConfiguration.Authentication)}:{nameof(SmtpConfiguration.Authentication.Username)}");
        }

        if (options.Authentication.Password.IsNullOrEmpty())
        {
            return InvalidOption($"{nameof(SmtpConfiguration.Authentication)}:{nameof(SmtpConfiguration.Authentication.Password)}");
        }

        if (options.From.Name.IsNullOrEmpty())
        {
            return InvalidOption($"{nameof(SmtpConfiguration.From)}:{nameof(SmtpConfiguration.From.Name)}");
        }

        if (options.From.Address.IsNullOrEmpty())
        {
            return InvalidOption($"{nameof(SmtpConfiguration.From)}:{nameof(SmtpConfiguration.From.Address)}");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"Smtp option '{optionName}' must be provided.");
    }
}
