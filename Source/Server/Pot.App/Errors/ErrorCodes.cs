namespace Pot.App.Errors;

public static class ErrorCodes
{
    /// <summary>A failure to authenticate: the credentials or the session were not accepted.</summary>
    public const string Auth = nameof(Auth);

    /// <summary>A request that a validation rule, or a rule about the request itself, refused.</summary>
    public const string Invalid = nameof(Invalid);

    /// <summary>The requested record does not exist, or is not visible to the caller's site.</summary>
    public const string NotFound = nameof(NotFound);

    /// <summary>A conflict caused by the state of the data rather than by a rule the endpoint decided on.</summary>
    public const string Conflict = nameof(Conflict);

    /// <summary>A rule about related data refused the operation.</summary>
    public const string Constraint = nameof(Constraint);

    /// <summary>A value that must be unique already exists.</summary>
    public const string Duplicate = nameof(Duplicate);

    /// <summary>The entity tag in an update no longer matches the stored record.</summary>
    public const string StaleEntityTag = nameof(StaleEntityTag);

    /// <summary>A database failure that is not a conflict.</summary>
    public const string Database = nameof(Database);

    /// <summary>The caller exceeded a rate limit.</summary>
    public const string TooManyRequests = nameof(TooManyRequests);
}
