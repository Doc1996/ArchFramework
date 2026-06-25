using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Stores principal sessions in memory.
/// </summary>
public sealed class MemoryPrincipalSessionStore : IPrincipalSessionStore
{
	private readonly Lock _lock = new();
	private readonly Dictionary<PrincipalSessionId, PrincipalSession> _sessions = [];

	public IReadOnlyList<PrincipalSession> Sessions
	{
		get
		{
			lock (_lock)
				return _sessions.Values.ToList();
		}
	}

	public Task<Result<PrincipalSession>> SaveAsync(PrincipalSession session, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(session);

		lock (_lock)
			_sessions[session.SessionId] = session;
		return Task.FromResult(Result<PrincipalSession>.Ok(session));
	}

	public Task<Result<PrincipalSession?>> LoadAsync(PrincipalSessionId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
		{
			_sessions.TryGetValue(id, out var session);
			return Task.FromResult(Result<PrincipalSession?>.Ok(session));
		}
	}

	public Task<Result> DeleteAsync(PrincipalSessionId id, CancellationToken token = default)
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
