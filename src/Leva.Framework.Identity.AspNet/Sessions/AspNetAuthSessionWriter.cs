using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Writes and deletes framework auth session identifiers on ASP.NET Core responses.
/// </summary>
public sealed class AspNetAuthSessionWriter(IOptions<AspNetIdentityOptions> options)
{
	private readonly AspNetIdentityOptions _options = options.Value;

	public void Write(HttpContext context, AuthSessionId sessionId)
	{
		ArgumentNullException.ThrowIfNull(context);
		var cookieOptions = new CookieOptions
		{
			HttpOnly = _options.HttpOnlyCookie,
			Secure = _options.SecureCookie,
			SameSite = _options.SameSite,
			Expires = DateTimeOffset.UtcNow.Add(_options.CookieLifetime),
		};
		context.Response.Cookies.Append(_options.SessionCookieName, sessionId.Value, cookieOptions);
	}

	public void Delete(HttpContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		var cookieOptions = new CookieOptions
		{
			HttpOnly = _options.HttpOnlyCookie,
			Secure = _options.SecureCookie,
			SameSite = _options.SameSite,
		};
		context.Response.Cookies.Delete(_options.SessionCookieName, cookieOptions);
	}
}
