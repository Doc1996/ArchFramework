using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Resolves the current auth session from a configured in-memory session id.
/// </summary>
public sealed class MemoryAuthSessionSource : AuthSessionSource
{
	private readonly Lock _lock = new();
	private readonly AuthSessionService _sessionService;
	private AuthSessionId? _sessionId;

	public MemoryAuthSessionSource(AuthSessionService sessionService)
	{
		ArgumentNullException.ThrowIfNull(sessionService);
		_sessionService = sessionService;
	}

	public AuthSessionId? SessionId
	{
		get
		{
			lock (_lock)
				return _sessionId;
		}
	}

	public void SetSession(AuthSessionId sessionId)
	{
		lock (_lock)
			_sessionId = sessionId;
	}

	public void ClearSession()
	{
		lock (_lock)
			_sessionId = null;
	}

	public override Task<Result<AuthSession?>> GetSessionAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var sessionId = SessionId;

		return sessionId.HasValue
			? _sessionService.LoadAsync(sessionId.Value, token)
			: Task.FromResult(Result<AuthSession?>.Ok(null));
	}
}
