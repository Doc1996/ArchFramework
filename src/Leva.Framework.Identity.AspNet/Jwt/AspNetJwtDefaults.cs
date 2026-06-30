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
	public const string PermissionClaimType = "framework:permission";
	public const string SessionIdClaimType = "framework:auth_session_id";
	public const string Issuer = "Framework";
	public const string Audience = "Framework";

	public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
	public static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(1);
}
