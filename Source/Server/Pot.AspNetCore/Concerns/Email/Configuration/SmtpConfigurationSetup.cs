using AllOverIt.Extensions;
using Microsoft.Extensions.Options;
using Pot.EmailSender.Configuration;

namespace Pot.AspNetCore.Concerns.Email.Configuration;

public class SmtpConfigurationSetup : IConfigureOptions<SmtpConfiguration>, IValidateOptions<SmtpConfiguration>
{
    private const string SectionName = "Smtp";

    private readonly IConfiguration _configuration;

    public SmtpConfigurationSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SmtpConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

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
