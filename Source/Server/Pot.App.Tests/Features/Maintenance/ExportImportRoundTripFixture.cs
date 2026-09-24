using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Features.Accounts.GetAll;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.App.Features.Maintenance.Export.Accounts;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Import.Reader;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Readers;
using Pot.App.Features.Maintenance.Metadata.Serializer;
using Pot.App.Features.Maintenance.Metadata.Writers;
using Pot.TestUtils;
using Shouldly;
using System.IO.Compression;

namespace Pot.App.Tests.Features.Maintenance;

public class ExportImportRoundTripFixture : PotFixtureBase
{
    // The exporter writes accounts.csv by name while AccountCsvRow reads it by
    // position. Adding or removing a column therefore re-indexes every later
    // property, and a missed re-index moves values between fields rather than
    // failing. This round-trip is the guard: it runs the real exporter's output
    // back through the real import mapping.
    [Fact]
    public async Task Should_Round_Trip_Accounts_Csv_Through_Export_And_Import_Mapping()
    {
        var account = new Output
        {
            RowId = Guid.NewGuid(),
            Etag = 7,
            Description = "Everyday",
            Balance = 100.5d,
            Reserved = 10.25d,
            TotalExpenseAccrued = 1.5d,
            DailyExpenseAccrual = 0.25d,
            StableExpenseAccrual = 0.75d,
            LinkedExpenses = 0,
            LinkedIncomes = 0
        };

        var accountsService = Substitute.For<IGetAllAccountsService>();
        accountsService
            .GetAllAccountsAsync(Arg.Any<CancellationToken>())
            .Returns([account]);

        var exporter = new AccountsExporter(accountsService);

        var content = await exporter.ExportAllAsync(CancellationToken.None);

        using var enumerator = new CsvRowEnumerator<AccountCsvRow, IAccountCsvRow>(new MemoryStream(content));

        // The enumerator owns a single-use reader, so materialise once before asserting.
        var rows = enumerator.ToArray();
        var row = rows.ShouldHaveSingleItem();

        row.RowId.ShouldBe(account.RowId);
        row.Description.ShouldBe(account.Description);
        row.Balance.ShouldBe(account.Balance);
        row.Reserved.ShouldBe(account.Reserved);
        row.TotalExpenseAccrued.ShouldBe(account.TotalExpenseAccrued);
        row.DailyExpenseAccrual.ShouldBe(account.DailyExpenseAccrual);
    }

    // The import gate compares the package version against MetadataBase.CurrentVersion, so a
    // version that is written but never bumped would let an incompatible package through.
    [Fact]
    public void Should_Write_The_Current_Metadata_Version_Into_The_Package()
    {
        var serializer = new MetadataSerializer(new MetadataWriterFactory(), new MetadataReaderFactory());

        var content = serializer.Serialize(new MetadataV4 { CreatedAt = DateTime.UtcNow });

        using var zipStream = new MemoryStream();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using var entryStream = archive.CreateEntry("metadata").Open();
            entryStream.Write(content);
        }

        zipStream.Position = 0;

        var reader = new ImportStreamReader(serializer, NullLogger<ImportStreamReader>.Instance);

        using (reader.Open(zipStream))
        {
            reader.ReadMetadataVersion().ShouldBe(MetadataBase.CurrentVersion);
        }
    }
}
