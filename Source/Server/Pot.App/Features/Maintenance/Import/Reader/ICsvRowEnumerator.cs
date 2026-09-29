namespace Pot.App.Features.Maintenance.Import.Reader;

/// <summary>
/// Enumerates the CSV rows of a package entry as a forward-only sequence of the requested row type.
/// </summary>
/// <typeparam name="TAs">The row abstraction exposed by the enumerator.</typeparam>
/// <remarks>
/// The underlying stream and reader are owned by the enumerator and released when it is disposed. Enumeration is
/// single-pass and cannot be restarted after disposal.
/// </remarks>
public interface ICsvRowEnumerator<TAs> : IEnumerable<TAs>, IDisposable
{
}
