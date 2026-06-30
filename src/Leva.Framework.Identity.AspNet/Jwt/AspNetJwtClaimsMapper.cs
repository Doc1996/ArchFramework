namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps framework auth sessions and principals to compact JWT claim payloads.
/// </summary>
internal sealed class AspNetJwtClaimsMapper
{
	internal AspNetJwtPayload Map(
		AuthSession session,
		string issuer,
		string audience,
		DateTimeOffset issuedAt,
		DateTimeOffset expiresAt
	)
	{
		ArgumentNullException.ThrowIfNull(session);
		return new AspNetJwtPayload(
			issuer,
			audience,
			session.Principal.Id.Value,
			session.Principal.DisplayName,
			session.Principal.Email,
			session.SessionId.Value,
			ToUnixTime(issuedAt),
			ToUnixTime(issuedAt),
			ToUnixTime(expiresAt),
			session.Principal.Roles.Select(role => role.Value).ToArray(),
			session.Principal.Permissions.Select(permission => permission.Value).ToArray(),
			session.Principal.Claims.Select(claim => new AspNetJwtClaim(claim.Type, claim.Value)).ToArray()
		);
	}

	internal AuthSession Map(AspNetJwtPayload payload)
	{
		ArgumentNullException.ThrowIfNull(payload);
		var principal = new Principal(
			new PrincipalId(payload.Subject),
			payload.Name,
			payload.Email,
			payload.Roles.Select(role => new PrincipalRole(role)).ToHashSet(),
			payload.Permissions.Select(permission => new PrincipalPermission(permission)).ToHashSet(),
			payload.Claims.Select(claim => new PrincipalClaim(claim.Type, claim.Value)).ToArray()
		);

		return new AuthSession(
			new AuthSessionId(payload.SessionId),
			principal,
			AuthSessionStatus.Active,
			FromUnixTime(payload.IssuedAt),
			FromUnixTime(payload.IssuedAt),
			FromUnixTime(payload.ExpiresAt)
		);
	}

	private static long ToUnixTime(DateTimeOffset value) => value.ToUnixTimeSeconds();

	private static DateTimeOffset FromUnixTime(long value) => DateTimeOffset.FromUnixTimeSeconds(value);
}
