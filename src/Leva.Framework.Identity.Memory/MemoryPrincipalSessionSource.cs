using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Resolves the current principal session from a configured in-memory session id.
/// </summary>
public sealed class MemoryPrincipalSessionSource : PrincipalSessionSource
{
	private readonly PrincipalSessionService _sessionService;
	private PrincipalSessionId? _sessionId;

	public MemoryPrincipalSessionSource(PrincipalSessionService sessionService)
	{
		ArgumentNullException.ThrowIfNull(sessionService);
		_sessionService = sessionService;
	}

	public PrincipalSessionId? SessionId => _sessionId;

	public void SetSession(PrincipalSessionId sessionId) => _sessionId = sessionId;

	public void ClearSession() => _sessionId = null;

	public override Task<Result<PrincipalSession?>> GetSessionAsync(CancellationToken token = default)
	{
		return _sessionId.HasValue
			? _sessionService.LoadAsync(_sessionId.Value, token)
			: Task.FromResult(Result<PrincipalSession?>.Ok(null));
	}
}
