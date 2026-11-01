using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Pot.AspNetCore.Concerns.Forwarding.Configuration;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Forwarding.Configuration;

public class ForwardedHeadersOptionsSetupFixture
{
    public class Configure : ForwardedHeadersOptionsSetupFixture
    {
        [Fact]
        public void Should_Process_X_Forwarded_Proto_So_The_Scheme_Is_Https_Behind_The_Ingress()
        {
            var options = ConfigureOptions();

            options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto).ShouldBeTrue();
        }

        [Fact]
        public void Should_Process_X_Forwarded_For_So_The_Remote_Address_Is_The_Caller()
        {
            var options = ConfigureOptions();

            options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor).ShouldBeTrue();
        }

        [Fact]
        public void Should_Not_Process_X_Forwarded_Host()
        {
            // The ingress passes the original Host through already. Honouring the header as well would let a caller
            // choose the host the API believes it is serving.
            var options = ConfigureOptions();

            options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost).ShouldBeFalse();
        }

        [Fact]
        public void Should_Clear_The_Known_Networks_So_The_Ingress_Is_Not_Rejected_As_Unknown()
        {
            // The default is loopback only. Left populated, the headers are read and then discarded - which looks exactly
            // like not configuring forwarded headers at all.
            var options = ConfigureOptions();

            options.KnownIPNetworks.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Clear_The_Known_Proxies_So_The_Ingress_Is_Not_Rejected_As_Unknown()
        {
            var options = ConfigureOptions();

            options.KnownProxies.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Leave_The_Forward_Limit_At_A_Single_Hop()
        {
            // Only the rightmost X-Forwarded-For entry - the one the ingress wrote - is used, so a caller cannot choose
            // its own address.
            var options = ConfigureOptions();

            options.ForwardLimit.ShouldBe(1);
        }

        [Fact]
        public void Should_Start_From_Defaults_That_Would_Discard_The_Headers()
        {
            // Guards the two Clear() tests above: if a future framework default were already empty, they would pass
            // whether or not the setup still cleared anything.
            var defaults = new ForwardedHeadersOptions();

            defaults.KnownIPNetworks.ShouldNotBeEmpty();
            defaults.KnownProxies.ShouldNotBeEmpty();
        }
    }

    private static ForwardedHeadersOptions ConfigureOptions()
    {
        var setup = new ForwardedHeadersOptionsSetup();
        var options = new ForwardedHeadersOptions();

        setup.Configure(options);

        return options;
    }
}
