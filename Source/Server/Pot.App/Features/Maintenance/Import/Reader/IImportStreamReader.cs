using Pot.App.Features.Maintenance.Import.Models;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Reader;

/// <summary>
/// Reads the entries of a maintenance export package that has been opened over a stream.
/// </summary>
/// <remarks>
/// Call <see cref="Open"/> before reading entries. Each entry accessor transfers ownership of the stream or row
/// enumerator it returns to the caller, which must dispose it.
/// </remarks>
public interface IImportStreamReader : IPotScopedDependency
{
    /// <summary>
    /// Gets the names of the entries in the opened package.
    /// </summary>
    /// <value>
    /// The entry names, or an empty array when no package has been opened.
    /// </value>
    string[] EntryNames { get; }

    /// <summary>
    /// Opens the package over the supplied stream.
    /// </summary>
    /// <param name="stream">The stream containing the package to read.</param>
    /// <returns>A disposable that releases the opened package and its stream when disposed.</returns>
    /// <remarks>
    /// The reader owns <paramref name="stream"/> and disposes it when the returned value is disposed.
    /// </remarks>
    IDisposable Open(Stream stream);

    /// <summary>
    /// Reads the metadata schema version recorded at the start of the package's metadata entry.
    /// </summary>
    /// <returns>The metadata schema version recorded in the package.</returns>
    int ReadMetadataVersion();

    // Forcing the caller to specify the expected metadata type since they must know the expected type
    // of metadata to read. The version is read separately to allow for version-specific handling
    // before reading the metadata.
    /// <summary>
    /// Reads the package metadata as the requested type.
    /// </summary>
    /// <typeparam name="TMetadata">The expected metadata type.</typeparam>
    /// <returns>The deserialized metadata.</returns>
    TMetadata GetMetadata<TMetadata>() where TMetadata : MetadataBase;

    // Note: ICsvRowEnumerator<T> is IDisposable
    /// <summary>
    /// Gets an enumerator over the account rows in the package.
    /// </summary>
    /// <returns>An enumerator over the account rows; the caller must dispose it.</returns>
    ICsvRowEnumerator<IAccountCsvRow> GetAccounts();

    /// <summary>
    /// Gets an enumerator over the expense rows in the package.
    /// </summary>
    /// <returns>An enumerator over the expense rows; the caller must dispose it.</returns>
    ICsvRowEnumerator<IExpenseCsvRow> GetExpenses();

    /// <summary>
    /// Gets an enumerator over the income rows in the package.
    /// </summary>
    /// <returns>An enumerator over the income rows; the caller must dispose it.</returns>
    ICsvRowEnumerator<IIncomeCsvRow> GetIncomes();
}
