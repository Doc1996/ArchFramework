namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Holds built in ASP.NET Core Google identity defaults.
/// </summary>
public static class AspNetGoogleDefaults
{
	public static AuthenticationMethod Method { get; } = new("google");

	public const string HttpClientName = "FrameworkGoogle";
	public const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
	public const string TokenEndpoint = "https://oauth2.googleapis.com/token";
	public const string UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";
	public const string Scope = "openid email profile";
	public const string StateCookieName = ".Framework.Google.State";
	public const string ReturnUrlCookieName = ".Framework.Google.ReturnUrl";
	public const string DefaultReturnUrl = "/";
	public const string SubjectClaimType = "google:subject";
	public const string PictureClaimType = "google:picture";
	public const int CorrelationLifetimeMinutes = 10;

	public static TimeSpan CorrelationLifetime => TimeSpan.FromMinutes(CorrelationLifetimeMinutes);
}
