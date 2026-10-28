namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// An operation the authorization matrix deliberately leaves alone, with the reason a case would be unsafe or
/// meaningless.
/// </summary>
/// <param name="Operation">The operation being left alone.</param>
/// <param name="Reason">Why no case is generated, written so a reviewer can judge whether it still holds.</param>
public sealed record IgnoredOperation(EndpointOperation Operation, string Reason);
