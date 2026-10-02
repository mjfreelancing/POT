using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Features.Accounts.GetAll;
using Pot.App.Features.Accounts.GetAll.Models;
using Pot.App.Features.Expenses.GetAll;
using Pot.App.Features.Maintenance.Export.Accounts;
using Pot.App.Features.Maintenance.Export.Expenses;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Import.Reader;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;
using System.IO.Compression;
using ExpensesOutput = Pot.App.Features.Expenses.GetAll.Models.Output;

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
            TotalArrears = 0.5d,
            TotalCommitted = 2.0d,
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
    }

    // expenses.csv carries the risk the accounts round trip does not: Accrued sat between Amount and
    // Note, so removing it shifts Note and AccountRowId up one position. A row model that still bound
    // them at their old indices would read the account id as the note and run past the end of the row
    // for the account id itself.
    [Fact]
    public async Task Should_Round_Trip_Expenses_Csv_Through_Export_And_Import_Mapping()
    {
        var accountRowId = Guid.NewGuid();

        var expense = new ExpensesOutput
        {
            RowId = Guid.NewGuid(),
            Etag = 3,
            ExcludeFromCalcs = false,
            Description = "Internet",
            AccrualStart = new DateOnly(2025, 1, 1),
            NextDue = new DateOnly(2025, 2, 1),
            EndDate = null,
            AccrualPolicy = AccrualPolicy.Automatic,
            Frequency = Frequency.Months,
            FrequencyCount = 1,
            Amount = 89.95d,
            Accrued = 12.5d,
            Arrears = 0.0d,
            Note = "Round-trip note",
            Account = new ExpensesOutput.AccountModel
            {
                RowId = accountRowId,
                Description = "Everyday"
            }
        };

        var expensesService = Substitute.For<IGetExpensesService>();
        expensesService
            .GetAllExpensesAsync(Arg.Any<CancellationToken>())
            .Returns([expense]);

        var exporter = new ExpensesExporter(expensesService);

        var content = await exporter.ExportAllAsync(CancellationToken.None);

        using var enumerator = new CsvRowEnumerator<ExpenseCsvRow, IExpenseCsvRow>(new MemoryStream(content));

        var rows = enumerator.ToArray();
        var row = rows.ShouldHaveSingleItem();

        row.RowId.ShouldBe(expense.RowId);
        row.Description.ShouldBe(expense.Description);
        row.AccrualStart.ShouldBe(expense.AccrualStart);
        row.NextDue.ShouldBe(expense.NextDue);
        row.Amount.ShouldBe(expense.Amount);
        row.Note.ShouldBe(expense.Note);
        row.AccountRowId.ShouldBe(accountRowId);
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
