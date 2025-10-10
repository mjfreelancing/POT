using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Pot.Data.Configuration;

// DbBackupConfigurationSetup is registered with ConfigureOptions in ServiceCollectionExtensions, and replaces the following code:
//
//   services.Configure<DbBackupConfiguration>(options => configuration.GetSection("Database").Bind(options))
//       .PostConfigure<DbBackupConfiguration>(options =>
//       {
//           // Validate required fields
//           _ = options.Host.WhenNotNullOrEmpty();
//           _ = options.Name.WhenNotNullOrEmpty();
//           _ = options.Username.WhenNotNullOrEmpty();
//           _ = options.Password.WhenNotNullOrEmpty();
//       });

public class DbBackupConfigurationSetup : IConfigureOptions<DatabaseConfiguration>, IValidateOptions<DatabaseConfiguration>
{
    private const string SectionName = "Database";

    private readonly IConfiguration _configuration;

    public DbBackupConfigurationSetup(IConfiguration configuration)
    {
        _configuration = configuration.WhenNotNull();
    }

    public void Configure(DatabaseConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

    public ValidateOptionsResult Validate(string? name, DatabaseConfiguration options)
    {
        if (options.Host.IsNullOrEmpty())
        {
            return InvalidOption(nameof(DatabaseConfiguration.Host));
        }

        if (options.Name.IsNullOrEmpty())
        {
            return InvalidOption(nameof(DatabaseConfiguration.Name));
        }

        if (options.Username.IsNullOrEmpty())
        {
            return InvalidOption(nameof(DatabaseConfiguration.Username));
        }

        if (options.Password.IsNullOrEmpty())
        {
            return InvalidOption(nameof(DatabaseConfiguration.Password));
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"Database configuration option '{optionName}' must be provided.");
    }
}