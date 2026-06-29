using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Stores auth sessions in memory.
/// </summary>
public sealed class MemoryAuthSessionStore : IAuthSessionStore
{
	private readonly SyncDictionary<AuthSessionId, AuthSession> _sessions = new();
	public IReadOnlyList<AuthSession> Sessions => _sessions.Values();

	public Task<Result<AuthSession>> SaveAsync(AuthSession session, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(session);

		_sessions.Set(session.SessionId, session);
		return Task.FromResult(Result<AuthSession>.Ok(session));
	}

	public Task<Result<AuthSession?>> LoadAsync(AuthSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(Result<AuthSession?>.Ok(_sessions.GetOrDefault(sessionId)));
	}

	public Task<Result> DeleteAsync(AuthSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.Remove(sessionId);
		return Task.FromResult(Result.Ok());
	}

	public void Clear() => _sessions.Clear();
}
