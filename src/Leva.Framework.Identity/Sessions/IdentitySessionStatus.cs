namespace Leva.Framework.Identity;

/// <summary>
/// Describes the lifecycle state of an identity session.
/// </summary>
public enum IdentitySessionStatus
{
	Active,
	Expired,
	SignedOut,
	Revoked,
}
