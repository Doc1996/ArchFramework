namespace Leva.Framework.Identity;

/// <summary>
/// Creates sessions, computes expiration, and marks sessions expired or signed out.
/// </summary>
public class AuthSessionPolicy(TimeSpan? lifetime = null)
{
	private readonly TimeSpan? _lifetime = lifetime ?? TimeSpan.FromHours(8);

	public virtual AuthSession Create(Principal principal, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(principal);
		return new AuthSession(
			AuthSessionId.New(),
			principal,
			AuthSessionStatus.Active,
			utcNow,
			utcNow,
			GetExpiration(utcNow)
		);
	}

	public virtual bool IsExpired(AuthSession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session.ExpiresAt.HasValue && session.ExpiresAt.Value <= utcNow;
	}

	public virtual AuthSession Expire(AuthSession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session with { Status = AuthSessionStatus.Expired, UpdatedAt = utcNow };
	}

	public virtual AuthSession SignOut(AuthSession session, DateTimeOffset utcNow)
	{
		ArgumentNullException.ThrowIfNull(session);
		return session with { Status = AuthSessionStatus.SignedOut, UpdatedAt = utcNow };
	}

	protected virtual DateTimeOffset? GetExpiration(DateTimeOffset utcNow) =>
		_lifetime.HasValue ? utcNow.Add(_lifetime.Value) : null;
}
