namespace Leva.Framework.Identity;

/// <summary>
/// Describes the lifecycle state of an principal session.
/// </summary>
public enum PrincipalSessionStatus
{
	Active,
	Expired,
	SignedOut,
	Revoked,
}
