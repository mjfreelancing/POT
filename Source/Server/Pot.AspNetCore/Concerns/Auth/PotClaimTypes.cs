namespace Pot.AspNetCore.Concerns.Auth;

internal static class PotClaimTypes
{
    public const string TokenVersion = "token_version";

    // The RowId of the AuthSession the access token was issued for, so revoking that session also cancels the token.
    public const string SessionId = "session_id";
}
