using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import.Reader;

public interface IImportStreamReaderFactory : IPotSingletonDependency
{
    IImportStreamReader CreateReader(Stream stream);
}
