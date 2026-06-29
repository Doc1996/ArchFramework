using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// In-memory principal session store fake for tests.
/// </summary>
public sealed class FakePrincipalSessionStore : IPrincipalSessionStore
{
	private readonly Dictionary<PrincipalSessionId, PrincipalSession> _sessions = [];
	public IReadOnlyDictionary<PrincipalSessionId, PrincipalSession> Sessions => _sessions;

	public Task<Result<PrincipalSession>> SaveAsync(PrincipalSession session, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(session);
		_sessions[session.SessionId] = session;

		return Task.FromResult(Result<PrincipalSession>.Ok(session));
	}

	public Task<Result<PrincipalSession?>> LoadAsync(PrincipalSessionId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.TryGetValue(id, out var session);
		return Task.FromResult(Result<PrincipalSession?>.Ok(session));
	}

	public Task<Result> DeleteAsync(PrincipalSessionId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_sessions.Remove(id);
		return Task.FromResult(Result.Ok());
	}
}
