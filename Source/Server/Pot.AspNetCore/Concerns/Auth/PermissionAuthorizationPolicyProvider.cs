using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Auth;

/*
    Dynamically adds new policies rather than having to explicitly configure them like this

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SiteManage", policy => policy.Requirements.Add(new PermissionRequirement("site:manage")));
    });
 */

internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}


