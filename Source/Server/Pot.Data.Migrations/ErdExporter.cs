using AllOverIt.Assertion;
using AllOverIt.EntityFrameworkCore.Diagrams;
using AllOverIt.EntityFrameworkCore.Diagrams.D2;
using AllOverIt.EntityFrameworkCore.Diagrams.D2.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.Data.Entities;
using System.Diagnostics;

namespace Pot.Data.Migrations;

internal sealed class ErdExporter
{
    private readonly IDbContextFactory<PotDbContext> _dbContextFactory;
    private readonly ILogger _logger;

    public ErdExporter(IDbContextFactory<PotDbContext> dbContextFactory, ILogger<ErdExporter> logger)
    {
        _dbContextFactory = dbContextFactory.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task ExportSchemaAsDiagramAsync(string exportFilename)
    {
        // Defaults are:
        // * Show non-nullable columns as [NOT NULL]
        // * Show MaxLength
        // * Show 1:1 cardinality as "ONE-TO-ONE"
        // * Show 1:N cardinality as "ONE-TO-MANY"
        // * Cardinality label style is as per D2 defaults
        var erdFormatter = ErdGenerator
            .Create<D2ErdGenerator>(options =>
            {
                options.Direction = ErdOptions.DiagramDirection.Right;

                // Add Role / Permissions group
                var rolePermGroupStyle = new ShapeStyle
                {
                    Fill = "#ffffcc",   // pale yellow
                    Stroke = "#cccc99"  // darker yellow
                };

                options.Group("role_perm", "Roles / Permissions", rolePermGroupStyle, entities =>
                {
                    entities
                        .Add<RoleEntity>()
                        .Add<PermissionEntity>()

                        // Shadow tables
                        .Add("UserRole")
                        .Add("RolePermission");
                });

                // Add Finance group
                var financeGroupStyle = new ShapeStyle
                {
                    Fill = "#ccffcc",   // pale green
                    Stroke = "#99cc99"  // darker green
                };

                options.Group("finance", "Finance", financeGroupStyle, entities =>
                {
                    entities
                        .Add<AccountEntity>()
                        .Add<ExpenseEntity>()
                        .Add<IncomeEntity>()
                        .Add<AccountAccrualEntity>();
                });

                // Add Site group
                var siteGroupStyle = new ShapeStyle
                {
                    Fill = "#ffcccc",   // pale red
                    Stroke = "#cc9999"  // darker red
                };

                options.Group("site", "Site", siteGroupStyle, entities =>
                {
                    entities
                        .Add<SiteEntity>()
                        .Add<SettingEntity>();
                });

                // Add Auth group
                var authGroup = new ShapeStyle
                {
                    Fill = "#ccccff",   // pale blue
                    Stroke = "#9999cc"  // darker blue
                };

                options.Group("auth", "Auth", authGroup, entities =>
                {
                    entities
                        .Add<UserEntity>()
                        .Add<AuthSessionEntity>()
                        .Add<OneTimePasswordEntity>();
                });

                // Define the global Entities options before creating the entity specific options (so they can be pre-filled).
                options.Entities.Nullable.IsVisible = true;
                options.Entities.Nullable.Mode = NullableColumnMode.NotNull;

                // This is the default
                options.Entities.ShowMaxLength = true;

                // Individual properties
                options.Cardinality.LabelStyle.FontSize = 24;
            });

        using var dbContext = _dbContextFactory.CreateDbContext();

        var erd = erdFormatter.Generate(dbContext);

        var exportOptions = new D2ErdExportOptions
        {
            DiagramFileName = exportFilename,
            LayoutEngine = "elk",
            Theme = Theme.Neutral,
            Formats = [ExportFormat.Png],
            StandardOutputHandler = LogOutput,
            ErrorOutputHandler = LogOutput          // Note: d2.exe logs everything to the error output
        };

        await erdFormatter.ExportAsync(dbContext, exportOptions);
    }

    private void LogOutput(object sender, DataReceivedEventArgs evt)
    {
        if (evt.Data is not null)
        {
            _logger.LogInformation("D2: {D2Data}", evt.Data);
        }
    }
}
