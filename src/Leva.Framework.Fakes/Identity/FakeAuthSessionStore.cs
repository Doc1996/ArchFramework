using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// In-memory auth session store fake for tests.
/// </summary>
public sealed class FakeAuthSessionStore : IAuthSessionStore
{
	private readonly Dictionary<AuthSessionId, AuthSession> _sessions = [];
	public IReadOnlyDictionary<AuthSessionId, AuthSession> Sessions => _sessions;

	public Task<Result<AuthSession>> SaveAsync(AuthSession session, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(session);
		_sessions[session.SessionId] = session;

		return Task.FromResult(Result<AuthSession>.Ok(session));
	}

	public Task<Result<AuthSession?>> LoadAsync(AuthSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.TryGetValue(sessionId, out var session);
		return Task.FromResult(Result<AuthSession?>.Ok(session));
	}

	public Task<Result> DeleteAsync(AuthSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.Remove(sessionId);
		return Task.FromResult(Result.Ok());
	}
}
