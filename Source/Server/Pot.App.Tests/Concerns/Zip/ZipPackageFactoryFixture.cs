using AllOverIt.Zip;
using Pot.App.Concerns.Zip;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Concerns.Zip;

public class ZipPackageFactoryFixture : PotFixtureBase
{
    public class CreateZipPackage : ZipPackageFactoryFixture
    {
        private readonly ZipPackageFactory _factory;

        public CreateZipPackage()
        {
            _factory = new ZipPackageFactory();
        }

        [Fact]
        public void Should_Return_ZipPackage_Instance()
        {
            var result = _factory.CreateZipPackage();

            result.ShouldNotBeNull();
            result.ShouldBeAssignableTo<IZipPackage>();
        }

        [Fact]
        public void Should_Return_New_Instance_Each_Time()
        {
            var result1 = _factory.CreateZipPackage();
            var result2 = _factory.CreateZipPackage();

            result1.ShouldNotBeSameAs(result2);
        }

        [Fact]
        public void Should_Return_ZipPackage_Type()
        {
            var result = _factory.CreateZipPackage();

            result.ShouldBeOfType<ZipPackage>();
        }
    }
}
