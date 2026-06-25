using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Selects authentication policies, creates sessions, handles sign-out, and writes audit entries.
/// </summary>
public sealed class AuthenticationService
{
	private readonly IReadOnlyList<AuthenticationPolicy> _policies;
	private readonly IdentitySessionService _sessionService;
	private readonly IAuditSink _auditSink;
	private readonly IClock? _clock;

	public AuthenticationService(
		IEnumerable<AuthenticationPolicy> policies,
		IdentitySessionService sessionService,
		IAuditSink? auditSink = null,
		IClock? clock = null
	)
	{
		ArgumentNullException.ThrowIfNull(policies);
		ArgumentNullException.ThrowIfNull(sessionService);

		_policies = policies.ToArray();
		_sessionService = sessionService;
		_auditSink = auditSink ?? NullAuditSink.Instance;
		_clock = clock;
	}

	public async Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		var policy = _policies.FirstOrDefault(policy => policy.CanAuthenticate(request));
		if (policy is null)
		{
			var error = IdentityErrors.NotFound($"authentication policy for '{request.Method}'");
			WriteFailure(request.Method, error.Message);
			return Result<AuthenticationResult>.Fail(error);
		}

		var result = await policy.AuthenticateAsync(request, token);
		if (result.IsFailure)
		{
			WriteFailure(request.Method, result.Error.Message);
			return result;
		}

		var authentication = result.Value!;
		if (!authentication.IsAuthenticated || authentication.Identity is null)
		{
			WriteFailure(request.Method, authentication.Reason ?? "Authentication failed.");
			return result;
		}

		var sessionResult = await _sessionService.CreateAsync(authentication.Identity, token);
		if (sessionResult.IsFailure)
		{
			WriteFailure(request.Method, sessionResult.Error.Message, authentication.Identity.Id);
			return Result<AuthenticationResult>.Fail(sessionResult.Error);
		}

		var finalResult = authentication.WithSession(sessionResult.Value!);
		WriteSuccess(request.Method, authentication.Identity.Id, sessionResult.Value!.SessionId);
		return Result<AuthenticationResult>.Ok(finalResult);
	}

	public async Task<Result> SignOutAsync(IdentitySessionId sessionId, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var result = await _sessionService.SignOutAsync(sessionId, token);
		return result.IsSuccess ? Result.Ok() : Result.Fail(result.Error);
	}

	private DateTimeOffset UtcNow => _clock?.UtcNow ?? DateTimeOffset.UtcNow;

	private void WriteSuccess(AuthenticationMethod method, IdentityId identityId, IdentitySessionId sessionId) =>
		_auditSink.Write(new AuditEntry(AuditAction.Authenticated, UtcNow, identityId, sessionId, method.ToString()));

	private void WriteFailure(AuthenticationMethod method, string reason, IdentityId? identityId = null) =>
		_auditSink.Write(
			new AuditEntry(
				AuditAction.AuthenticationFailed,
				UtcNow,
				identityId,
				Method: method.ToString(),
				Reason: reason
			)
		);
}
