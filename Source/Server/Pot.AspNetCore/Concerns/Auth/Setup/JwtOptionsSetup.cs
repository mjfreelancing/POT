using Microsoft.Extensions.Options;
using Pot.AspNetCore.Concerns.Auth.Models;

namespace Pot.AspNetCore.Concerns.Auth.Setup;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private const string SectionName = "Jwt";

    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}
