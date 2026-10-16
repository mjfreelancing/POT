using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Approvals.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddApprovalEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Platform Routes]"))
        {
            app.Logger.LogInformation("Adding platform endpoints");

            var group = app
                .MapGroup(ApprovalsEndpoints.Group)
                .WithTags(ApprovalsEndpoints.Tag)
                .ProducesStandardProblems()
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetPendingApprovals()
                .UpdateUserApproval();
        }

        return app;
    }
}
