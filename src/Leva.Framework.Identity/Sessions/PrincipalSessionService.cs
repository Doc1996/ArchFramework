using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Creates, loads, signs out, expires, deletes, and audits principal sessions.
/// </summary>
public sealed class PrincipalSessionService
{
	private readonly IPrincipalSessionStore _sessionStore;
	private readonly PrincipalSessionPolicy _policy;
	private readonly IAuditSink _auditSink;
	private readonly IClock? _clock;

	public PrincipalSessionService(
		IPrincipalSessionStore sessionStore,
		PrincipalSessionPolicy? policy = null,
		IAuditSink? auditSink = null,
		IClock? clock = null
	)
	{
		ArgumentNullException.ThrowIfNull(sessionStore);

		_sessionStore = sessionStore;
		_policy = policy ?? new PrincipalSessionPolicy();
		_auditSink = auditSink ?? new NullAuditSink();
		_clock = clock;
	}

	public async Task<Result<PrincipalSession>> CreateAsync(Principal principal, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(principal);

		var session = _policy.Create(principal, UtcNow);
		var result = await _sessionStore.SaveAsync(session, token);

		if (result.IsSuccess)
			Write(AuditAction.SessionCreated, session);
		return result;
	}

	public async Task<Result<PrincipalSession?>> LoadAsync(
		PrincipalSessionId sessionId,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.LoadAsync(sessionId, token);

		if (result.IsFailure || result.Value is null)
			return result;

		var session = result.Value;
		if (!_policy.IsExpired(session, UtcNow))
			return result;

		var expired = _policy.Expire(session, UtcNow);
		await _sessionStore.SaveAsync(expired, token);

		Write(AuditAction.SessionExpired, expired);
		return Result<PrincipalSession?>.Ok(null);
	}

	public async Task<Result<PrincipalSession>> SignOutAsync(
		PrincipalSessionId sessionId,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.LoadAsync(sessionId, token);

		if (result.IsFailure)
			return Result<PrincipalSession>.Fail(result.Error);
		if (result.Value is null)
			return Result<PrincipalSession>.Fail(PrincipalErrors.NotFound("principal session", sessionId.ToString()));

		var signedOut = _policy.SignOut(result.Value, UtcNow);
		var save = await _sessionStore.SaveAsync(signedOut, token);

		if (save.IsSuccess)
			Write(AuditAction.SignedOut, signedOut);
		return save;
	}

	public async Task<Result<PrincipalSession>> ExpireAsync(
		PrincipalSessionId sessionId,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.LoadAsync(sessionId, token);

		if (result.IsFailure)
			return Result<PrincipalSession>.Fail(result.Error);
		if (result.Value is null)
			return Result<PrincipalSession>.Fail(PrincipalErrors.NotFound("principal session", sessionId.ToString()));

		var expired = _policy.Expire(result.Value, UtcNow);
		var save = await _sessionStore.SaveAsync(expired, token);

		if (save.IsSuccess)
			Write(AuditAction.SessionExpired, expired);
		return save;
	}

	public async Task<Result> DeleteAsync(PrincipalSessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.DeleteAsync(sessionId, token);

		if (result.IsSuccess)
			Write(AuditAction.SessionDeleted, sessionId: sessionId);
		return result;
	}

	private DateTimeOffset UtcNow => _clock?.UtcNow ?? DateTimeOffset.UtcNow;

	private void Write(AuditAction action, PrincipalSession? session = null, PrincipalSessionId? sessionId = null) =>
		_auditSink.Write(new AuditEntry(action, UtcNow, session?.Principal.Id, session?.SessionId ?? sessionId));
}
