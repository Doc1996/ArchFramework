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
	public PathString CallbackPath { get; set; } =
		AspNetGoogleEndpoints.DefaultPrefix + AspNetGoogleEndpoints.CallbackPath;
	public string AuthorizationEndpoint { get; set; } = AspNetGoogleDefaults.AuthorizationEndpoint;
	public string TokenEndpoint { get; set; } = AspNetGoogleDefaults.TokenEndpoint;
	public string UserInfoEndpoint { get; set; } = AspNetGoogleDefaults.UserInfoEndpoint;
	public string Scope { get; set; } = AspNetGoogleDefaults.Scope;
	public string StateCookieName { get; set; } = AspNetGoogleDefaults.StateCookieName;
	public string ReturnUrlCookieName { get; set; } = AspNetGoogleDefaults.ReturnUrlCookieName;
	public string DefaultReturnUrl { get; set; } = AspNetGoogleDefaults.DefaultReturnUrl;
	public bool SecureCookie { get; set; } = true;
	public bool HttpOnlyCookie { get; set; } = true;
	public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
	public TimeSpan CorrelationLifetime { get; set; } = AspNetGoogleDefaults.CorrelationLifetime;
}
