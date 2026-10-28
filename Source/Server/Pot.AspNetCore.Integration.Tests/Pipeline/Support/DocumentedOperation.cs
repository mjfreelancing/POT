namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// One operation the contract documents, with the part of its shape an authorization case depends on.
/// </summary>
/// <param name="Operation">The operation, named by method and path.</param>
/// <param name="CarriesRequestBody">
/// Whether the operation documents a request body. A case sends one only when it does, and only a request without a
/// body can be held to the row lookup that answers an absent row.
/// </param>
public sealed record DocumentedOperation(EndpointOperation Operation, bool CarriesRequestBody);
