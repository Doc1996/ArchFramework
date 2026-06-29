namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Request body used by the built in ASP.NET Core identity login endpoint.
/// </summary>
public sealed record AspNetLoginRequest(
	string Method,
	string? Name = null,
	string? Secret = null,
	string? Token = null,
	IReadOnlyDictionary<string, string>? Properties = null
)
{
	public AuthenticationRequest ToAuthenticationRequest() =>
		new(new AuthenticationMethod(Method), Name, Secret, Token, Properties);
}
