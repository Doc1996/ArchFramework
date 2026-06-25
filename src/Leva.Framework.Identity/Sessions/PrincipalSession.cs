namespace Leva.Framework.Identity;

/// <summary>
/// Represents one established principal session with lifecycle status and timestamps.
/// </summary>
public sealed record PrincipalSession(
	PrincipalSessionId SessionId,
	Principal Principal,
	PrincipalSessionStatus Status,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt,
	DateTimeOffset? ExpiresAt = null
)
{
	public bool IsActive => Status == PrincipalSessionStatus.Active;
}
