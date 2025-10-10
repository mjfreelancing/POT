using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Features.DbBackup.Configuration;

// BackupConfigurationSetup is registered with ConfigureOptions in ServiceCollectionExtensions, and replaces the following code:
//
//   services.Configure<BackupConfiguration>(options => configuration.GetSection("Database").Bind(options))
//       .PostConfigure<BackupConfiguration>(options =>
//       {
//           // Validate required fields
//           _ = options.BackupPath.WhenNotNullOrEmpty();

//           options.FilePrefix = environment.IsProduction() ? "prod" : "dev";
//       });

public class BackupConfigurationSetup : IConfigureOptions<BackupConfiguration>,
                                        IPostConfigureOptions<BackupConfiguration>,
                                        IValidateOptions<BackupConfiguration>
{
    private const string SectionName = "Database";

    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public BackupConfigurationSetup(IHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment.WhenNotNull();
        _configuration = configuration.WhenNotNull();
    }

    public void Configure(BackupConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

    public void PostConfigure(string? name, BackupConfiguration options)
    {
        options.FilePrefix = _environment.IsProduction() ? "prod" : "dev";
    }

    public ValidateOptionsResult Validate(string? name, BackupConfiguration options)
    {
        if (options.BackupPath.IsNullOrEmpty())
        {
            return InvalidOption(nameof(BackupConfiguration.BackupPath));
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"Backup configuration option '{optionName}' must be provided.");
    }
}

