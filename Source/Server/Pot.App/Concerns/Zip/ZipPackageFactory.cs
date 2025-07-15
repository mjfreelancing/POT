using AllOverIt.Zip;

namespace Pot.App.Concerns.Zip;

internal sealed class ZipPackageFactory : IZipPackageFactory
{
    public IZipPackage CreateZipPackage()
    {
        return new ZipPackage();
    }
}
