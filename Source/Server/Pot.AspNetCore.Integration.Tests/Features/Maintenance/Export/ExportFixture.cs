using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Maintenance.Export;

public class ExportFixture : IntegrationAuthFixtureBase
{
    private const string ExportPath = "/api/maintenance/export";
    private const string ImportPath = "/api/maintenance/import";
    private const string AccountsPath = "/api/accounts";
    private const string ExpensesPath = "/api/expenses";
    private const string RoundTripNote = "Round-trip note";
    private const double SeededAmount = 89.95d;

    private static readonly DateOnly SeededAccrualStart = new(2025, 1, 1);

    private sealed class IdentifiedResponse
    {
        public Guid RowId { get; set; }
    }

    private sealed class ImportResponse
    {
        public int Imported { get; set; }
    }

    private sealed class ExpenseResponse
    {
        public Guid RowId { get; set; }
        public DateOnly? AccrualStart { get; set; }
        public double Amount { get; set; }
        public string? Note { get; set; }
        public AccountIdentifier? Account { get; set; }

        public sealed class AccountIdentifier
        {
            public Guid RowId { get; set; }
        }
    }

    // No token is sent, so this covers the authorization boundary without any data setup.
    [Fact]
    public async Task Should_Return_Unauthorized_When_Requesting_Export_Without_Authentication()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(ExportPath, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_A_Versioned_File_Name_When_Requesting_Export()
    {
        var admin = await CreateAdminUserAsync("export", "Export User");

        using var client = await CreateAuthenticatedClientAsync(admin, "POT Export Test Agent/1.0");

        var response = await client.GetAsync(ExportPath, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/octet-stream");

        var contentDisposition = response.Content.Headers.ContentDisposition;

        contentDisposition.ShouldNotBeNull();
        contentDisposition!.DispositionType.ShouldBe("attachment");
        contentDisposition.FileName.ShouldNotBeNullOrWhiteSpace();
        contentDisposition.FileName!
            .Trim('"')
            .ShouldMatch($@"^pot-\d{{4}}-\d{{2}}-\d{{2}}_\d{{6}}\.v{MetadataBase.CurrentVersion}\.export$");

        var content = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        content.ShouldNotBeEmpty();
    }

    // accounts.csv and expenses.csv are read back by position, so the narrower column set is only
    // proven by adding the exported package back. expenses.csv is the risky one: Accrued sat between
    // Amount and Note, so removing it shifts Note and AccountRowId, and an old row model binding the
    // old indices would move values between fields rather than fail.
    [Fact]
    public async Task Should_Export_The_Narrower_Header_Set_And_Accept_It_Back_On_Import()
    {
        var admin = await CreateAdminUserAsync("export", "Export User");

        using var client = await CreateAuthenticatedClientAsync(admin, "POT Export Test Agent/1.0");

        var (accountRowId, expenseRowId) = await SeedAccountWithExpenseAsync(client);

        var exportResponse = await client.GetAsync(ExportPath, TestContext.Current.CancellationToken);

        exportResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var package = await exportResponse.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        AssertPackageHeader(package, "accounts", "RowId", "Description", "Balance", "Reserved");

        AssertPackageHeader(package, "expenses", "RowId", "ExcludeFromCalcs", "Description", "AccrualStart", "NextDue",
            "EndDate", "AccrualPolicy", "Frequency", "FrequencyCount", "Amount", "Note", "AccountRowId");

        using var importContent = new MultipartFormDataContent();

        importContent.Add(new ByteArrayContent(package), "File", "round-trip.export");

        var importResponse = await client.PostAsync(ImportPath, importContent, TestContext.Current.CancellationToken);

        importResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var importBody = await importResponse.Content.ReadFromJsonAsync<ImportResponse>(TestContext.Current.CancellationToken);

        importBody.ShouldNotBeNull();
        importBody!.Imported.ShouldBe(2, "the seeded account and its expense are both imported");

        var expensesResponse = await client.GetAsync(ExpensesPath, TestContext.Current.CancellationToken);

        expensesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var expenses = await expensesResponse.Content.ReadFromJsonAsync<ExpenseResponse[]>(TestContext.Current.CancellationToken);

        expenses.ShouldNotBeNull();

        var roundTripped = expenses!.ShouldHaveSingleItem();

        roundTripped.RowId.ShouldBe(expenseRowId);
        roundTripped.Amount.ShouldBe(SeededAmount);
        roundTripped.AccrualStart.ShouldBe(SeededAccrualStart);
        roundTripped.Note.ShouldBe(RoundTripNote);
        roundTripped.Account.ShouldNotBeNull();
        roundTripped.Account!.RowId.ShouldBe(accountRowId);
    }

    // Returns the account and expense rowIds it created.
    private static async Task<(Guid AccountRowId, Guid ExpenseRowId)> SeedAccountWithExpenseAsync(HttpClient client)
    {
        var accountRequest = new
        {
            Description = $"Round-trip Account {Guid.NewGuid():N}",
            Balance = 1000.0d,
            Reserved = 100.0d
        };

        var accountResponse = await client.PostAsJsonAsync(AccountsPath, accountRequest, TestContext.Current.CancellationToken);

        accountResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var account = await accountResponse.Content.ReadFromJsonAsync<IdentifiedResponse>(TestContext.Current.CancellationToken);

        account.ShouldNotBeNull();

        var expenseRequest = new
        {
            Description = "Round-trip Expense",
            AccrualStart = SeededAccrualStart.ToString("yyyy-MM-dd"),
            NextDue = "2025-02-01",
            EndDate = (string?)null,
            AccrualPolicy = "Automatic",
            Frequency = "Months",
            FrequencyCount = 1,
            Amount = SeededAmount,
            Note = RoundTripNote,
            AccountRowId = account!.RowId
        };

        var expenseResponse = await client.PostAsJsonAsync(ExpensesPath, expenseRequest, TestContext.Current.CancellationToken);

        expenseResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var expense = await expenseResponse.Content.ReadFromJsonAsync<IdentifiedResponse>(TestContext.Current.CancellationToken);

        expense.ShouldNotBeNull();

        return (account.RowId, expense!.RowId);
    }

    private static void AssertPackageHeader(byte[] package, string entryName, params string[] expectedColumns)
    {
        using var stream = new MemoryStream(package);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var entry = archive.GetEntry(entryName);

        entry.ShouldNotBeNull($"the package contains a {entryName} entry");

        using var reader = new StreamReader(entry!.Open());

        var header = reader.ReadLine();

        header.ShouldNotBeNullOrWhiteSpace();

        header!.Split(',', StringSplitOptions.TrimEntries).ShouldBe(expectedColumns);

        // The derived accrual values are no longer written. AccrualStart and AccrualPolicy are inputs
        // and stay, so only the past-tense name is checked.
        header.ShouldNotContain("Accrued", Case.Insensitive);
    }

}
