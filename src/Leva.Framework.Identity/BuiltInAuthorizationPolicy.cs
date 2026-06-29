using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Authorizes signed-in, role, permission, and claim requirements from the principal model.
/// </summary>
public sealed class BuiltInAuthorizationPolicy : AuthorizationPolicy
{
	private const string RoleType = "Role";
	private const string PermissionType = "Permission";
	private const string ClaimPrefix = "Claim:";

	public override bool CanAuthorize(AuthorizationRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		return request.Requirement == AuthorizationRequirement.SignedIn
			|| request.Requirement.Type == RoleType
			|| request.Requirement.Type == PermissionType
			|| IsClaimRequirement(request.Requirement);
	}

	public override Task<Result<AuthorizationResult>> AuthorizeAsync(
		AuthorizationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		var result = Authorize(request);
		return Task.FromResult(Result<AuthorizationResult>.Ok(result));
	}

	private static AuthorizationResult Authorize(AuthorizationRequest request)
	{
		if (request.Principal is null)
			return AuthorizationResult.Denied(request.Requirement, "Principal is not available.");

		if (request.Requirement == AuthorizationRequirement.SignedIn)
			return request.Session is { SessionStatus: AuthSessionStatus.Active }
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(request.Requirement, "Principal is not signed in.");

		if (request.Requirement.Type == RoleType)
			return HasRole(request.Principal, request.Requirement.Value)
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(
					request.Requirement,
					$"Principal does not have role '{request.Requirement.Value}'."
				);

		if (request.Requirement.Type == PermissionType)
			return HasPermission(request.Principal, request.Requirement.Value)
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(
					request.Requirement,
					$"Principal does not have permission '{request.Requirement.Value}'."
				);

		if (IsClaimRequirement(request.Requirement))
		{
			var claimType = GetClaimType(request.Requirement);
			return HasClaim(request.Principal, claimType, request.Requirement.Value)
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(request.Requirement, $"Principal does not have claim '{claimType}'.");
		}

		return AuthorizationResult.Denied(
			request.Requirement,
			$"Requirement '{request.Requirement}' is not supported."
		);
	}

	private static bool IsClaimRequirement(AuthorizationRequirement requirement) =>
		requirement.Type.StartsWith(ClaimPrefix, StringComparison.Ordinal);

	private static string GetClaimType(AuthorizationRequirement requirement) => requirement.Type[ClaimPrefix.Length..];

	private static bool HasRole(Principal principal, string value) =>
		principal.Roles.Any(role => string.Equals(role.Value, value, StringComparison.Ordinal));

	private static bool HasPermission(Principal principal, string value) =>
		principal.Permissions.Any(permission => string.Equals(permission.Value, value, StringComparison.Ordinal));

	private static bool HasClaim(Principal principal, string type, string value) =>
		principal.Claims.Any(claim =>
			string.Equals(claim.Type, type, StringComparison.Ordinal)
			&& string.Equals(claim.Value, value, StringComparison.Ordinal)
		);
}
