namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// Who is calling in an authorization case.
/// </summary>
public enum AuthorizationCaller
{
    /// <summary>A client carrying no credentials.</summary>
    Anonymous,

    /// <summary>A signed-in user holding the administrator role in the tenant the run acts in.</summary>
    Administrator,

    /// <summary>A signed-in user holding the viewer role in the tenant the run acts in.</summary>
    Viewer
}
