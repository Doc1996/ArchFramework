namespace Leva.Framework.Identity;

/// <summary>
/// Creates sessions, computes expiration, and marks sessions expired or signed out.
/// </summary>
public class PrincipalSessionPolicy(TimeSpan? lifetime = null)
{
	private readonly TimeSpan? _lifetime = lifetime ?? TimeSpan.FromHours(8);

	public virtual PrincipalSession Create(Principal principal, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(principal);
		return new PrincipalSession(
			PrincipalSessionId.New(),
			principal,
			PrincipalSessionStatus.Active,
			utcNow,
			utcNow,
			GetExpiration(utcNow)
		);
	}

	public virtual bool IsExpired(PrincipalSession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session.ExpiresAt.HasValue && session.ExpiresAt.Value <= utcNow;
	}

	public virtual PrincipalSession Expire(PrincipalSession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session with { Status = PrincipalSessionStatus.Expired, UpdatedAt = utcNow };
	}

	public virtual PrincipalSession SignOut(PrincipalSession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session with { Status = PrincipalSessionStatus.SignedOut, UpdatedAt = utcNow };
	}

	protected virtual DateTimeOffset? GetExpiration(DateTimeOffset utcNow) =>
		_lifetime.HasValue ? utcNow.Add(_lifetime.Value) : null;
}
