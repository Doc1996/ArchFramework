namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Response body returned by the built in ASP.NET Core identity login endpoint.
/// </summary>
public sealed record AspNetLoginResponse(
	bool IsAuthenticated,
	string? PrincipalId = null,
	string? DisplayName = null,
	string? Email = null,
	string? Reason = null
)
{
	public static AspNetLoginResponse From(AuthenticationResult result) =>
		new(
			result.IsAuthenticated,
			result.Principal?.Id.Value,
			result.Principal?.DisplayName,
			result.Principal?.Email,
			result.Reason
		);
}
