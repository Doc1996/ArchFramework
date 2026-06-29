namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Response body returned by the built in ASP.NET Core current principal endpoint.
/// </summary>
public sealed record AspNetCurrentPrincipalResponse(
	bool IsAuthenticated,
	string? SessionId = null,
	string? PrincipalId = null,
	string? DisplayName = null,
	string? Email = null,
	IReadOnlyList<string>? Roles = null,
	IReadOnlyList<string>? Permissions = null,
	IReadOnlyList<PrincipalClaim>? Claims = null
)
{
	public static AspNetCurrentPrincipalResponse Anonymous { get; } = new(false);

	public static AspNetCurrentPrincipalResponse From(AuthSession session) =>
		new(
			true,
			session.SessionId.Value,
			session.Principal.Id.Value,
			session.Principal.DisplayName,
			session.Principal.Email,
			session.Principal.Roles.Select(role => role.Value).ToArray(),
			session.Principal.Permissions.Select(permission => permission.Value).ToArray(),
			session.Principal.Claims.ToArray()
		);
}
