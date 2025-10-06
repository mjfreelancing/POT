namespace Pot.App.Features.Maintenance.Import.Reader;

public interface IImportStreamReader : IDisposable
{
    string[] EntryNames { get; }

    // Will throw if the entry does not exist.
    // The caller must dispose the stream.
    Stream GetEntry(string name);
}
