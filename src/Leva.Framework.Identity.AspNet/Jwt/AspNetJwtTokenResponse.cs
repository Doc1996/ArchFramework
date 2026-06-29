namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Response body returned by the built in ASP.NET Core JWT login endpoint.
/// </summary>
public sealed record AspNetJwtTokenResponse(
	string AccessToken,
	string TokenType,
	DateTimeOffset ExpiresAt,
	AspNetLoginResponse Principal
);
