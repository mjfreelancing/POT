namespace Pot.App.Features.Maintenance.Import.Reader;

public interface ICsvRowEnumerator<TAs> : IEnumerable<TAs>, IDisposable
{
}
