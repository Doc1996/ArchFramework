using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Runs applicable authorization policies and writes audit entries.
/// </summary>
public sealed class AuthorizationService
{
	private readonly IReadOnlyList<AuthorizationPolicy> _policies;
	private readonly IAuditSink _auditSink;
	private readonly IClock? _clock;

	public AuthorizationService(
		IEnumerable<AuthorizationPolicy> policies,
		IAuditSink? auditSink = null,
		IClock? clock = null
	)
	{
		ArgumentNullException.ThrowIfNull(policies);

		_policies = policies.ToArray();
		_auditSink = auditSink ?? new NullAuditSink();
		_clock = clock;
	}

	public async Task<Result<AuthorizationResult>> AuthorizeAsync(
		AuthorizationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		var policies = _policies.Where(policy => policy.CanAuthorize(request)).ToArray();
		if (policies.Length == 0)
		{
			var error = PrincipalErrors.NotFound($"authorization policy for '{request.Requirement}'");
			Write(AuditAction.AuthorizationFailed, request, error.Message);
			return Result<AuthorizationResult>.Fail(error);
		}

		var lastReason = "Authorization denied.";
		foreach (var policy in policies)
		{
			var result = await policy.AuthorizeAsync(request, token);
			if (result.IsFailure)
				return result;

			var authorization = result.Value!;
			if (authorization.IsAuthorized)
			{
				Write(AuditAction.Authorized, request, authorization.Reason);
				return result;
			}

			lastReason = authorization.Reason ?? lastReason;
		}

		Write(AuditAction.AuthorizationFailed, request, lastReason);
		return Result<AuthorizationResult>.Ok(AuthorizationResult.Denied(request.Requirement, lastReason));
	}

	private DateTimeOffset UtcNow => _clock?.UtcNow ?? DateTimeOffset.UtcNow;

	private void Write(AuditAction action, AuthorizationRequest request, string? reason) =>
		_auditSink.Write(
			new AuditEntry(
				action,
				UtcNow,
				request.Principal?.Id,
				request.Session?.SessionId,
				Requirement: request.Requirement.ToString(),
				Reason: reason
			)
		);
}
