namespace Leva.Framework.Identity;

/// <summary>
/// Describes one security-relevant principal action.
/// </summary>
public enum AuditAction
{
	Authenticated,
	AuthenticationFailed,
	SignedOut,
	SessionCreated,
	SessionExpired,
	SessionDeleted,
	Authorized,
	AuthorizationFailed,
}
