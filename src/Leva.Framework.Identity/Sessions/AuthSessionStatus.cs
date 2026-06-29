namespace Leva.Framework.Identity;

/// <summary>
/// Describes the lifecycle state of an auth session.
/// </summary>
public enum AuthSessionStatus
{
	Active,
	Expired,
	SignedOut,
	Revoked,
}
