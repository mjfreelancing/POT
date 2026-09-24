using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Maintenance.Import;
using Pot.App.Features.Maintenance.Import.Accounts;
using Pot.App.Features.Maintenance.Import.Expenses;
using Pot.App.Features.Maintenance.Import.Incomes;
using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Import.Reader;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Data;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Maintenance;

public class ImportVersionGateFixture : PotFixtureBase
{
    // Only the current version is read, so a package produced by any other build must be
    // refused. The important part is that nothing is imported: the importer's row types are
    // positional, so reading a superseded column set would move values between fields rather
    // than failing loudly.
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Should_Reject_A_Package_Whose_Metadata_Version_Is_Not_Current(int packageVersion)
    {
        var streamReader = Substitute.For<IImportStreamReader>();
        streamReader.EntryNames.Returns(["metadata", "accounts", "incomes", "expenses"]);
        streamReader.ReadMetadataVersion().Returns(packageVersion);

        var accountsImporter = Substitute.For<IAccountsImporter>();
        var incomesImporter = Substitute.For<IIncomesImporter>();
        var expensesImporter = Substitute.For<IExpensesImporter>();

        var service = new ImportDataService(
            streamReader,
            accountsImporter,
            incomesImporter,
            expensesImporter,
            Substitute.For<IPotTransactionFactory>(),
            NullLogger<ImportDataService>.Instance);

        var result = await service.ImportAsync(new MemoryStream(), CancellationToken.None);

        result.IsFail.ShouldBeTrue();

        var error = result.Error.ShouldBeOfType<ApiDetailError>();

        error.PropertyName.ShouldBe(nameof(MetadataBase.Version));
        error.AttemptedValue.ShouldBe(packageVersion);
        error.ErrorMessage.ShouldBe(
            $"The import file has a non-supported version. Expecting version {MetadataBase.CurrentVersion}.");

        // The metadata must not even be deserialised, and no rows may be read or written.
        streamReader
            .DidNotReceive()
            .GetMetadata<MetadataV4>();

        await accountsImporter
            .DidNotReceive()
            .ImportAsync(Arg.Any<IEnumerable<IAccountCsvRow>>(), Arg.Any<CancellationToken>());

        await incomesImporter
            .DidNotReceive()
            .ImportAsync(Arg.Any<IEnumerable<IIncomeCsvRow>>(), Arg.Any<CancellationToken>());

        await expensesImporter
            .DidNotReceive()
            .ImportAsync(Arg.Any<IEnumerable<IExpenseCsvRow>>(), Arg.Any<CancellationToken>());
    }
}
