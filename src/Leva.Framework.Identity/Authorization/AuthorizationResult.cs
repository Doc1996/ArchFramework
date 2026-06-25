namespace Leva.Framework.Identity;

/// <summary>
/// Represents the outcome of an authorization check.
/// </summary>
public sealed record AuthorizationResult(bool IsAuthorized, AuthorizationRequirement Requirement, string? Reason = null)
{
	public static AuthorizationResult Allowed(AuthorizationRequirement requirement) => new(true, requirement);

	public static AuthorizationResult Denied(AuthorizationRequirement requirement, string reason) =>
		new(false, requirement, reason);
}
