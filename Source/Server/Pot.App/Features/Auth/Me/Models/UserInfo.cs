namespace Pot.App.Features.Auth.Me.Models;

public sealed record UserInfo(Guid UserId, string Username, string DisplayName, string Email);
