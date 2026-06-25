namespace Leva.Framework.Identity;

/// <summary>
/// Creates sessions, computes expiration, and marks sessions expired or signed out.
/// </summary>
public class IdentitySessionPolicy(TimeSpan? lifetime = null)
{
	private readonly TimeSpan? _lifetime = lifetime ?? TimeSpan.FromHours(8);

	public virtual IdentitySession Create(Identity identity, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(identity);
		return new IdentitySession(
			IdentitySessionId.New(),
			identity,
			IdentitySessionStatus.Active,
			utcNow,
			utcNow,
			GetExpiration(utcNow)
		);
	}

	public virtual bool IsExpired(IdentitySession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session.ExpiresAt.HasValue && session.ExpiresAt.Value <= utcNow;
	}

	public virtual IdentitySession Expire(IdentitySession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session with { Status = IdentitySessionStatus.Expired, UpdatedAt = utcNow };
	}

	public virtual IdentitySession SignOut(IdentitySession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session with { Status = IdentitySessionStatus.SignedOut, UpdatedAt = utcNow };
	}

	protected virtual DateTimeOffset? GetExpiration(DateTimeOffset utcNow) =>
		_lifetime.HasValue ? utcNow.Add(_lifetime.Value) : null;
}
