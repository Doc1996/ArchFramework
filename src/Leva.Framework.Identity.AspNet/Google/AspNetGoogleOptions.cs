using Microsoft.AspNetCore.Http;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Configures built in ASP.NET Core Google sign-in endpoints and token validation.
/// </summary>
public sealed class AspNetGoogleOptions
{
	public AuthenticationMethod Method { get; set; } = AspNetGoogleDefaults.Method;
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public PathString CallbackPath { get; set; } = "/identity/google/callback";
	public string AuthorizationEndpoint { get; set; } = AspNetGoogleDefaults.AuthorizationEndpoint;
	public string TokenEndpoint { get; set; } = AspNetGoogleDefaults.TokenEndpoint;
	public string UserInfoEndpoint { get; set; } = AspNetGoogleDefaults.UserInfoEndpoint;
	public string Scope { get; set; } = "openid email profile";
	public string StateCookieName { get; set; } = ".Framework.Google.State";
	public string ReturnUrlCookieName { get; set; } = ".Framework.Google.ReturnUrl";
	public string DefaultReturnUrl { get; set; } = "/";
	public bool SecureCookie { get; set; } = true;
	public bool HttpOnlyCookie { get; set; } = true;
	public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
	public TimeSpan CorrelationLifetime { get; set; } = TimeSpan.FromMinutes(10);
}
