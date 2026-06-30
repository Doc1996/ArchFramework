namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Holds built in ASP.NET Core identity defaults.
/// </summary>
public static class AspNetDefaults
{
	public const string AuthenticationScheme = "FrameworkIdentity";
	public const string SessionCookieName = ".Framework.AuthSession";
	public const string SessionHeaderName = "Auth-Session";
	public const int CookieLifetimeDays = 14;

	public static TimeSpan CookieLifetime => TimeSpan.FromDays(CookieLifetimeDays);
}
