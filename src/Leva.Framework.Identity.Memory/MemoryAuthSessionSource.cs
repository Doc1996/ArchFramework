using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Resolves the current auth session from a configured in-memory session id.
/// </summary>
public sealed class MemoryAuthSessionSource : AuthSessionSource
{
	private readonly AuthSessionService _sessionService;
	private AuthSessionId? _sessionId;

	public MemoryAuthSessionSource(AuthSessionService sessionService)
	{
		ArgumentNullException.ThrowIfNull(sessionService);
		_sessionService = sessionService;
	}

	public AuthSessionId? SessionId => _sessionId;

	public void SetSession(AuthSessionId sessionId) => _sessionId = sessionId;

	public void ClearSession() => _sessionId = null;

	public override Task<Result<AuthSession?>> GetSessionAsync(CancellationToken token = default)
	{
		return _sessionId.HasValue
			? _sessionService.LoadAsync(_sessionId.Value, token)
			: Task.FromResult(Result<AuthSession?>.Ok(null));
	}
}
