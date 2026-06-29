namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Holds built in ASP.NET Core Google identity defaults.
/// </summary>
public static class AspNetGoogleDefaults
{
	public static AuthenticationMethod Method { get; } = new("google");

	public const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
	public const string TokenEndpoint = "https://oauth2.googleapis.com/token";
	public const string UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";
	public const string SubjectClaimType = "google:subject";
	public const string PictureClaimType = "google:picture";
}
