using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Creates, loads, signs out, expires, deletes, and audits identity sessions.
/// </summary>
public sealed class IdentitySessionService
{
	private readonly IIdentitySessionStore _sessionStore;
	private readonly IdentitySessionPolicy _policy;
	private readonly IAuditSink _auditSink;
	private readonly IClock? _clock;

	public IdentitySessionService(
		IIdentitySessionStore sessionStore,
		IdentitySessionPolicy? policy = null,
		IAuditSink? auditSink = null,
		IClock? clock = null
	)
	{
		ArgumentNullException.ThrowIfNull(sessionStore);

		_sessionStore = sessionStore;
		_policy = policy ?? new IdentitySessionPolicy();
		_auditSink = auditSink ?? new NullAuditSink();
		_clock = clock;
	}

	public async Task<Result<IdentitySession>> CreateAsync(Identity identity, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(identity);

		var session = _policy.Create(identity, UtcNow);
		var result = await _sessionStore.SaveAsync(session, token);

		if (result.IsSuccess)
			Write(AuditAction.SessionCreated, session);
		return result;
	}

	public async Task<Result<IdentitySession?>> LoadAsync(
		IdentitySessionId sessionId,
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
		return Result<IdentitySession?>.Ok(null);
	}

	public async Task<Result<IdentitySession>> SignOutAsync(
		IdentitySessionId sessionId,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.LoadAsync(sessionId, token);

		if (result.IsFailure)
			return Result<IdentitySession>.Fail(result.Error);
		if (result.Value is null)
			return Result<IdentitySession>.Fail(IdentityErrors.NotFound("identity session", sessionId.ToString()));

		var signedOut = _policy.SignOut(result.Value, UtcNow);
		var save = await _sessionStore.SaveAsync(signedOut, token);

		if (save.IsSuccess)
			Write(AuditAction.SignedOut, signedOut);
		return save;
	}

	public async Task<Result<IdentitySession>> ExpireAsync(
		IdentitySessionId sessionId,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.LoadAsync(sessionId, token);

		if (result.IsFailure)
			return Result<IdentitySession>.Fail(result.Error);
		if (result.Value is null)
			return Result<IdentitySession>.Fail(IdentityErrors.NotFound("identity session", sessionId.ToString()));

		var expired = _policy.Expire(result.Value, UtcNow);
		var save = await _sessionStore.SaveAsync(expired, token);

		if (save.IsSuccess)
			Write(AuditAction.SessionExpired, expired);
		return save;
	}

	public async Task<Result> DeleteAsync(IdentitySessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionStore.DeleteAsync(sessionId, token);

		if (result.IsSuccess)
			Write(AuditAction.SessionDeleted, sessionId: sessionId);
		return result;
	}

	private DateTimeOffset UtcNow => _clock?.UtcNow ?? DateTimeOffset.UtcNow;

	private void Write(AuditAction action, IdentitySession? session = null, IdentitySessionId? sessionId = null) =>
		_auditSink.Write(new AuditEntry(action, UtcNow, session?.Identity.Id, session?.SessionId ?? sessionId));
}
