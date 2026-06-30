using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps framework principals and auth sessions to ASP.NET Core claims principals.
/// </summary>
public sealed class AspNetClaimsPrincipalMapper(IOptions<AspNetIdentityOptions> options)
{
	public const string SessionIdClaimType = "framework:auth_session_id";
	public const string PermissionClaimType = "framework:permission";

	private readonly AspNetIdentityOptions _options = options.Value;

	public ClaimsPrincipal Map(AuthSession session, string? authenticationType = null)
	{
		ArgumentNullException.ThrowIfNull(session);
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, session.Principal.Id.Value),
			new(ClaimTypes.Name, session.Principal.DisplayName),
			new(SessionIdClaimType, session.SessionId.Value),
		};

		if (!string.IsNullOrWhiteSpace(session.Principal.Email))
			claims.Add(new Claim(ClaimTypes.Email, session.Principal.Email));

		claims.AddRange(session.Principal.Claims.Select(claim => new Claim(claim.Type, claim.Value)));
		claims.AddRange(session.Principal.Roles.Select(role => new Claim(ClaimTypes.Role, role.Value)));
		claims.AddRange(
			session.Principal.Permissions.Select(permission => new Claim(PermissionClaimType, permission.Value))
		);

		var scheme = authenticationType ?? _options.AuthenticationScheme;
		return new ClaimsPrincipal(new ClaimsIdentity(claims, scheme, ClaimTypes.Name, ClaimTypes.Role));
	}

	public AuthSessionId? GetSessionId(ClaimsPrincipal principal)
	{
		ArgumentNullException.ThrowIfNull(principal);
		var sessionId = principal.FindFirstValue(SessionIdClaimType);
		return string.IsNullOrWhiteSpace(sessionId) ? null : new AuthSessionId(sessionId);
	}

	public PrincipalId? GetPrincipalId(ClaimsPrincipal principal)
	{
		ArgumentNullException.ThrowIfNull(principal);
		var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
		return string.IsNullOrWhiteSpace(principalId) ? null : new PrincipalId(principalId);
	}
}
