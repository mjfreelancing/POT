namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// An entity the data model declares that belongs to no tenant, together with the reason it is shared.
/// </summary>
/// <param name="Entity">The entity type, which must be one the data model declares.</param>
/// <param name="Reason">Why the table cannot be attributed to a site, written so a reviewer can judge it.</param>
public sealed record GlobalEntity(Type Entity, string Reason);
