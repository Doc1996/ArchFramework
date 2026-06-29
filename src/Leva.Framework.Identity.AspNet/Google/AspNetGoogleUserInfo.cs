namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Represents Google user information used to create framework principals.
/// </summary>
public sealed record AspNetGoogleUserInfo(
	string Subject,
	string DisplayName,
	string? Email = null,
	bool EmailVerified = false,
	string? Picture = null
);
