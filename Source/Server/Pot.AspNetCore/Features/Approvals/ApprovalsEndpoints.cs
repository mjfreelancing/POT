namespace Pot.AspNetCore.Features.Approvals;

internal static class ApprovalsEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/approvals";
    public const string Tag = "Platform Api";

    public const string PendingApprovals = "/pending";
    public const string UpdateStatus = "/{id:guid}/status";
}
