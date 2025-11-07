using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Cors.Configuration;

// Specifically used to configure CorsOptions after CorsConfiguration has been set up
public sealed class CorsOptionsSetup : IConfigureOptions<CorsOptions>
{
    private readonly CorsConfiguration _corsConfiguration;

    public CorsOptionsSetup(CorsConfiguration corsConfiguration)
    {
        _corsConfiguration = corsConfiguration;
    }

    public void Configure(CorsOptions options)
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                // Allow frontend URLs
                .WithOrigins(_corsConfiguration.AllowedOrigins)

                .AllowAnyMethod()
                .AllowAnyHeader()

                // Required when using authentication (note, WithOrigins() cannot use * with AllowCredentials)
                .AllowCredentials()

                // Exposing 'content-disposition' allows the client to handle the file download correctly (get the filename)
                .WithExposedHeaders("content-disposition");
        });
    }
}