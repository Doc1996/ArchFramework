using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Reads framework auth session identifiers from ASP.NET Core requests.
/// </summary>
public sealed class AspNetAuthSessionReader(IOptions<AspNetIdentityOptions> options)
{
	private readonly AspNetIdentityOptions _options = options.Value;

	public AuthSessionId? Read(HttpContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		var cookie = context.Request.Cookies[_options.SessionCookieName];

		if (!string.IsNullOrWhiteSpace(cookie))
			return new AuthSessionId(cookie);

		if (!_options.AllowHeaderSession)
			return null;

		var header = context.Request.Headers[_options.SessionHeaderName].FirstOrDefault();
		return string.IsNullOrWhiteSpace(header) ? null : new AuthSessionId(header);
	}
}
