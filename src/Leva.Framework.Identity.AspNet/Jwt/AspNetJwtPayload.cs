namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Represents the built in JWT payload used by framework auth sessions.
/// </summary>
public sealed record AspNetJwtPayload(
	string Issuer,
	string Audience,
	string Subject,
	string Name,
	string? Email,
	string SessionId,
	long IssuedAt,
	long NotBefore,
	long ExpiresAt,
	IReadOnlyList<string> Roles,
	IReadOnlyList<string> Permissions,
	IReadOnlyList<AspNetJwtClaim> Claims
);
