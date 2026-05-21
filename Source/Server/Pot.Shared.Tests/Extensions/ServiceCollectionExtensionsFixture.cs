using Microsoft.Extensions.DependencyInjection;
using Pot.Shared.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Shared.Tests.Extensions;

public class ServiceCollectionExtensionsFixture : PotFixtureBase
{
    private sealed class TestOptions
    {
        public string Value { get; set; } = string.Empty;
    }

    public class AddSingletonFromOptions : ServiceCollectionExtensionsFixture
    {
        [Fact]
        public void Should_Resolve_TType_Directly_From_Configured_Options()
        {
            var services = new ServiceCollection();

            services.Configure<TestOptions>(options => options.Value = "expected-value");
            services.AddSingletonFromOptions<TestOptions>();

            var provider = services.BuildServiceProvider();
            var resolved = provider.GetRequiredService<TestOptions>();

            resolved.Value.ShouldBe("expected-value");
        }
    }
}
