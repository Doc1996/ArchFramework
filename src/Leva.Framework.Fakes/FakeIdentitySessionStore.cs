using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// In-memory identity session store fake for tests.
/// </summary>
public sealed class FakeIdentitySessionStore : IIdentitySessionStore
{
	private readonly Dictionary<IdentitySessionId, IdentitySession> _sessions = [];
	public IReadOnlyDictionary<IdentitySessionId, IdentitySession> Sessions => _sessions;

	public Task<Result<IdentitySession>> SaveAsync(IdentitySession session, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(session);
		_sessions[session.SessionId] = session;

		return Task.FromResult(Result<IdentitySession>.Ok(session));
	}

	public Task<Result<IdentitySession?>> LoadAsync(IdentitySessionId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.TryGetValue(id, out var session);
		return Task.FromResult(Result<IdentitySession?>.Ok(session));
	}

	public Task<Result> DeleteAsync(IdentitySessionId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.Remove(id);
		return Task.FromResult(Result.Ok());
	}
}
