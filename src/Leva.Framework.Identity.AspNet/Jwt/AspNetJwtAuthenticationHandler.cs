using System.Security.Claims;
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
	IOptions<AspNetJwtOptions> jwtOptions,
	AspNetJwtTokenService tokens
) : AuthenticationHandler<AspNetJwtAuthenticationOptions>(options, logger, encoder)
{
	private readonly AspNetJwtOptions _jwtOptions = jwtOptions.Value;

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var token = ReadBearerToken();
		if (string.IsNullOrWhiteSpace(token))
			return Task.FromResult(AuthenticateResult.NoResult());

		var result = tokens.ValidateToken(token);
		if (result.IsFailure)
			return Task.FromResult(AuthenticateResult.Fail(result.Error.Message));

		var principal = Map(result.Value!);
		return Task.FromResult(
			AuthenticateResult.Success(new AuthenticationTicket(principal, _jwtOptions.AuthenticationScheme))
		);
	}

	private string? ReadBearerToken()
	{
		var header = Request.Headers.Authorization.FirstOrDefault();
		return header is not null && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
			? header["Bearer ".Length..].Trim()
			: null;
	}

	private ClaimsPrincipal Map(AuthSession session)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, session.Principal.Id.Value),
			new(ClaimTypes.Name, session.Principal.DisplayName),
			new(AspNetJwtDefaults.SessionIdClaimType, session.SessionId.Value),
		};

		if (!string.IsNullOrWhiteSpace(session.Principal.Email))
			claims.Add(new Claim(ClaimTypes.Email, session.Principal.Email));

		claims.AddRange(session.Principal.Roles.Select(role => new Claim(ClaimTypes.Role, role.Value)));
		claims.AddRange(
			session.Principal.Permissions.Select(permission => new Claim(
				AspNetJwtDefaults.PermissionClaimType,
				permission.Value
			))
		);
		claims.AddRange(session.Principal.Claims.Select(claim => new Claim(claim.Type, claim.Value)));

		return new ClaimsPrincipal(
			new ClaimsIdentity(claims, _jwtOptions.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role)
		);
	}
}
