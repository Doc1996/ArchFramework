namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Holds built in ASP.NET Core JWT identity defaults.
/// </summary>
public static class AspNetJwtDefaults
{
	public static AuthenticationMethod Method { get; } = new("jwt");

	public const string AuthenticationScheme = "FrameworkJwt";
	public const string TokenType = "Bearer";
	public const string AuthorizationPrefix = "Bearer ";
	public const string Issuer = "Framework";
	public const string Audience = "Framework";
	public const int TokenLifetimeMinutes = 60;
	public const int ClockSkewMinutes = 1;

	public static TimeSpan TokenLifetime => TimeSpan.FromMinutes(TokenLifetimeMinutes);
	public static TimeSpan ClockSkew => TimeSpan.FromMinutes(ClockSkewMinutes);
}
