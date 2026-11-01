using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Forwarding.Configuration;

// The folder is "Forwarding" rather than "ForwardedHeaders": a namespace segment of that name would shadow the
// ForwardedHeaders enum used below, and every use of it would have to be fully qualified.
//
// Nothing is bound from configuration, so unlike CorsOptionsSetup there is no settings type and no validation. The
// values are a property of the hosting platform, not of the environment.
/// <summary>Configures forwarded header processing for running behind the Azure Container Apps ingress.</summary>
public sealed class ForwardedHeadersOptionsSetup : IConfigureOptions<ForwardedHeadersOptions>
{
    /// <summary>Applies the forwarded header settings to the supplied options instance.</summary>
    /// <param name="options">The options instance to configure.</param>
    public void Configure(ForwardedHeadersOptions options)
    {
        // The ingress terminates TLS and forwards plain HTTP to the container. Without these two headers:
        //  - Request.Scheme is "http", so the Location header on every 201 Created is an http:// URL
        //  - Connection.RemoteIpAddress is the ingress, so every anonymous caller shares one rate-limit partition and
        //    every login records the ingress as the session's IP address
        //
        // X-Forwarded-Host is deliberately not processed. The ingress passes through the Host header it received, and
        // honouring the header as well would let a caller choose the host the API believes it is serving.
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        // The defaults trust loopback only, and the ingress is not at a fixed, enumerable address. Left as they are, the
        // headers would be read and then discarded - which looks exactly like not having configured any of this.
        //
        // Clearing both means trusting whoever sent the headers. That is safe only because nothing reaches the container
        // except through the ingress. If the container is ever exposed directly this must be revisited, because a caller
        // could then choose its own IP address and scheme.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        // ForwardLimit stays at its default of 1, so only the rightmost X-Forwarded-For entry - the one the ingress
        // wrote - is used, and nothing a caller puts in the header can be chosen as their address. Callers reach the
        // ingress directly (there is no CDN or Front Door in front of it), so that entry is the caller. If another proxy
        // is ever placed in front of the ingress, the entry becomes that proxy's address and this needs revisiting.
    }
}
