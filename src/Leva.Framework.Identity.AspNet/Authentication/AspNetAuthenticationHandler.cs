using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// ASP.NET Core authentication handler backed by framework auth sessions.
/// </summary>
public sealed class AspNetAuthenticationHandler(
	IOptionsMonitor<AspNetAuthenticationOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	AspNetAuthSessionSource sessionSource,
	AspNetClaimsPrincipalMapper principalMapper
) : AuthenticationHandler<AspNetAuthenticationOptions>(options, logger, encoder)
{
	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var session = await sessionSource.GetSessionAsync(Context.RequestAborted);
		if (session.IsFailure)
			return AuthenticateResult.Fail(session.Error.Message);

		if (session.Value is not { SessionStatus: AuthSessionStatus.Active } activeSession)
			return AuthenticateResult.NoResult();

		var principal = principalMapper.Map(activeSession);
		return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
	}
}
