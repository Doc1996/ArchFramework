using Microsoft.AspNetCore.Http;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Configures ASP.NET Core identity authentication scheme, auth session transport, and cookie behavior.
/// </summary>
public sealed class AspNetIdentityOptions
{
	public string AuthenticationScheme { get; set; } = "FrameworkIdentity";
	public string SessionCookieName { get; set; } = ".Framework.AuthSession";
	public string SessionHeaderName { get; set; } = "X-Auth-Session";
	public bool AllowHeaderSession { get; set; }
	public bool SecureCookie { get; set; } = true;
	public bool HttpOnlyCookie { get; set; } = true;
	public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
	public TimeSpan CookieLifetime { get; set; } = TimeSpan.FromDays(14);
}
