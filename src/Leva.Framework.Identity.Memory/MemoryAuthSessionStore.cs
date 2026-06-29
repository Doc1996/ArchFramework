using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Stores auth sessions in memory.
/// </summary>
public sealed class MemoryAuthSessionStore : IAuthSessionStore
{
	private readonly Lock _lock = new();
	private readonly Dictionary<AuthSessionId, AuthSession> _sessions = [];

	public IReadOnlyList<AuthSession> Sessions
	{
		get
		{
			lock (_lock)
				return _sessions.Values.ToList();
		}
	}

	public Task<Result<AuthSession>> SaveAsync(AuthSession session, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(session);

		lock (_lock)
			_sessions[session.SessionId] = session;
		return Task.FromResult(Result<AuthSession>.Ok(session));
	}

	public Task<Result<AuthSession?>> LoadAsync(AuthSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
		{
			_sessions.TryGetValue(id, out var session);
			return Task.FromResult(Result<AuthSession?>.Ok(session));
		}
	}

	public Task<Result> DeleteAsync(AuthSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
			_sessions.Remove(id);
		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
			_sessions.Clear();
	}
}
