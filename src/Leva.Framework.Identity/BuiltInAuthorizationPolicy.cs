using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Authorizes signed-in, role, permission, and claim requirements from the principal model.
/// </summary>
public sealed class BuiltInAuthorizationPolicy : AuthorizationPolicy
{
	public override bool CanAuthorize(AuthorizationRequest request)
	{
		return request.Requirement == AuthorizationRequirement.SignedIn
			|| request.Requirement.Type == "Role"
			|| request.Requirement.Type == "Permission"
			|| request.Requirement.Type.StartsWith("Claim:", StringComparison.Ordinal);
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
			return request.Session is { Status: AuthSessionStatus.Active }
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(request.Requirement, "Principal is not signed in.");

		if (request.Requirement.Type == "Role")
			return HasRole(request.Principal, request.Requirement.Value)
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(
					request.Requirement,
					$"Principal does not have role '{request.Requirement.Value}'."
				);

		if (request.Requirement.Type == "Permission")
			return HasPermission(request.Principal, request.Requirement.Value)
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(
					request.Requirement,
					$"Principal does not have permission '{request.Requirement.Value}'."
				);

		if (request.Requirement.Type.StartsWith("Claim:", StringComparison.Ordinal))
			return HasClaim(request.Principal, request.Requirement.Type[6..], request.Requirement.Value)
				? AuthorizationResult.Allowed(request.Requirement)
				: AuthorizationResult.Denied(
					request.Requirement,
					$"Principal does not have claim '{request.Requirement}'."
				);

		return AuthorizationResult.Denied(
			request.Requirement,
			$"Requirement '{request.Requirement}' is not supported."
		);
	}

	private static bool HasRole(Principal principal, string value) =>
		principal.Roles.Any(role => string.Equals(role.Value, value, StringComparison.Ordinal));

	private static bool HasPermission(Principal principal, string value) =>
		principal.Permissions.Any(permission => string.Equals(permission.Value, value, StringComparison.Ordinal));

	private static bool HasClaim(Principal principal, string type, string value) =>
		principal.Claims.Any(claim => claim.Type == type && claim.Value == value);
}
