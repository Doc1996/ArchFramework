namespace Leva.Framework.Identity;

/// <summary>
/// Represents one established auth session with lifecycle status and timestamps.
/// </summary>
public sealed record AuthSession(
	AuthSessionId SessionId,
	Principal Principal,
	AuthSessionStatus SessionStatus,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt,
	DateTimeOffset? ExpiresAt = null
)
{
	public bool IsActive => SessionStatus == AuthSessionStatus.Active;
}
