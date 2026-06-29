namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Holds built in ASP.NET Core JWT identity defaults.
/// </summary>
public static class AspNetJwtDefaults
{
	public static AuthenticationMethod Method { get; } = new("jwt");

	public const string AuthenticationScheme = "FrameworkJwt";
	public const string PermissionClaimType = "framework:permission";
	public const string SessionIdClaimType = "framework:auth_session_id";
}
