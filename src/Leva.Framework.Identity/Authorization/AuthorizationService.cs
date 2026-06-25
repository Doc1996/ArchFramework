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
		_auditSink = auditSink ?? NullAuditSink.Instance;
		_clock = clock;
	}

	public async Task<Result<AuthorizationResult>> AuthorizeAsync(
		AuthorizationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		if (request.Identity is null)
			return Deny(request, "Identity is not available.");

		var policies = _policies.Where(policy => policy.CanAuthorize(request)).ToArray();
		if (policies.Length == 0)
			return Deny(request, $"No authorization policy can evaluate '{request.Requirement}'.");

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

		return Deny(request, lastReason);
	}

	private DateTimeOffset UtcNow => _clock?.UtcNow ?? DateTimeOffset.UtcNow;

	private Result<AuthorizationResult> Deny(AuthorizationRequest request, string reason)
	{
		var result = AuthorizationResult.Denied(request.Requirement, reason);
		Write(AuditAction.AuthorizationFailed, request, reason);
		return Result<AuthorizationResult>.Ok(result);
	}

	private void Write(AuditAction action, AuthorizationRequest request, string? reason) =>
		_auditSink.Write(
			new AuditEntry(
				action,
				UtcNow,
				request.Identity?.Id,
				request.Session?.Id,
				Requirement: request.Requirement.ToString(),
				Reason: reason
			)
		);
}
