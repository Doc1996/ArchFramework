namespace Leva.Framework.Identity;

/// <summary>
/// Represents one established identity session with lifecycle status and timestamps.
/// </summary>
public sealed record IdentitySession(
	IdentitySessionId SesionId,
	Identity Identity,
	IdentitySessionStatus Status,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt,
	DateTimeOffset? ExpiresAt = null
)
{
	public bool IsActive => Status == IdentitySessionStatus.Active;
}
