using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// ASP.NET Core authentication handler backed by built in JWT bearer tokens.
/// </summary>
public sealed class AspNetJwtAuthenticationHandler(
	IOptionsMonitor<AspNetJwtAuthenticationOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	IOptions<AspNetJwtOptions> options,
	AspNetJwtTokenService tokenService,
	AspNetClaimsPrincipalMapper principalMapper
) : AuthenticationHandler<AspNetJwtAuthenticationOptions>(options, logger, encoder)
{
	private readonly AspNetJwtOptions _options = options.Value;

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var token = ReadBearerToken();
		if (string.IsNullOrWhiteSpace(token))
			return Task.FromResult(AuthenticateResult.NoResult());

		var session = tokenService.ValidateToken(token);
		if (session.IsFailure)
			return Task.FromResult(AuthenticateResult.Fail(session.Error.Message));

		var principal = principalMapper.Map(session.Value!, _options.AuthenticationScheme);
		var ticket = new AuthenticationTicket(principal, _options.AuthenticationScheme);
		return Task.FromResult(AuthenticateResult.Success(ticket));
	}

	private string? ReadBearerToken()
	{
		var header = Request.Headers.Authorization.FirstOrDefault();
		return
			header is not null
			&& header.StartsWith(AspNetJwtDefaults.AuthorizationPrefix, StringComparison.OrdinalIgnoreCase)
			? header[AspNetJwtDefaults.AuthorizationPrefix.Length..].Trim()
			: null;
	}
}
