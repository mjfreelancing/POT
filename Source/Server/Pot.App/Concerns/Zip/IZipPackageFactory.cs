using AllOverIt.Zip;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Zip;

public interface IZipPackageFactory : IPotScopedDependency
{
    IZipPackage CreateZipPackage();
}
