using Microsoft.AspNetCore.Http;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Configures ASP.NET Core identity authentication scheme, auth session transport, and cookie behavior.
/// </summary>
public sealed class AspNetIdentityOptions
{
	public string AuthenticationScheme { get; set; } = AspNetDefaults.AuthenticationScheme;
	public string SessionCookieName { get; set; } = AspNetDefaults.SessionCookieName;
	public string SessionHeaderName { get; set; } = AspNetDefaults.SessionHeaderName;
	public bool AllowHeaderSession { get; set; }
	public bool SecureCookie { get; set; } = true;
	public bool HttpOnlyCookie { get; set; } = true;
	public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
	public TimeSpan CookieLifetime { get; set; } = AspNetDefaults.CookieLifetime;
}
