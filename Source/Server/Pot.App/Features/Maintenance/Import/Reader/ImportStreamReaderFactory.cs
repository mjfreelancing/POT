using AllOverIt.Assertion;
using Microsoft.Extensions.Logging;

namespace Pot.App.Features.Maintenance.Import.Reader;

internal sealed class ImportStreamReaderFactory : IImportStreamReaderFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ImportStreamReaderFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory.WhenNotNull();
    }

    public IImportStreamReader CreateReader(Stream stream)
    {
        var logger = _loggerFactory.CreateLogger<ImportStreamReader>();

        return new ImportStreamReader(stream, logger);
    }
}
