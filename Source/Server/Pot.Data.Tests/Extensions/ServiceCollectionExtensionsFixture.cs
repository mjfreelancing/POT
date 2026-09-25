using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Settings;
using Pot.Data.Repositories.Users;
using Pot.Shared;
using Pot.Shared.DependencyInjection;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Extensions;

public class ServiceCollectionExtensionsFixture : PotFixtureBase
{
    public class AddDataDependencies : ServiceCollectionExtensionsFixture
    {
        [Fact]
        public void Should_Register_The_Repositories()
        {
            using var provider = CreateProvider(services => services.AddDataDependencies());
            using var scope = provider.CreateScope();

            scope.ServiceProvider.GetRequiredService<ISettingsRepository>().ShouldBeOfType<SettingsRepository>();
            scope.ServiceProvider.GetRequiredService<IUserRepository>().ShouldBeOfType<UserRepository>();
            scope.ServiceProvider.GetRequiredService<IAccountRepository>().ShouldBeOfType<AccountRepository>();
        }

        [Fact]
        public void Should_Register_The_Transaction_Factory()
        {
            using var provider = CreateProvider(services => services.AddDataDependencies());
            using var scope = provider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IPotTransactionFactory>().ShouldBeOfType<PotTransactionFactory>();
        }

        [Fact]
        public void Should_Not_Register_The_Dependency_Marker_Interface()
        {
            using var provider = CreateProvider(services => services.AddDataDependencies());

            provider.GetService<IPotScopedDependency>().ShouldBeNull();
        }
    }

    private static ServiceProvider CreateProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<PotDbContext>(options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        services.AddScoped(_ => Substitute.For<ICurrentUserContext>());

        configure(services);

        return services.BuildServiceProvider();
    }
}
