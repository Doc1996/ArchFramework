using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// State-facing principal capability that resolves the current auth session and performs authorization checks.
/// </summary>
public sealed class PrincipalAccess
{
	private readonly AuthSessionSource _sessionSource;
	private readonly AuthorizationService _authorization;

	public PrincipalAccess(AuthSessionSource sessionSource, AuthorizationService authorization)
	{
		ArgumentNullException.ThrowIfNull(sessionSource);
		ArgumentNullException.ThrowIfNull(authorization);

		_sessionSource = sessionSource;
		_authorization = authorization;
	}

	public Task<Result<AuthSession?>> GetSessionAsync(CancellationToken token = default) =>
		_sessionSource.GetSessionAsync(token);

	public async Task<Result<Principal?>> GetPrincipalAsync(CancellationToken token = default)
	{
		var session = await GetSessionAsync(token);
		return session.IsFailure
			? Result<Principal?>.Fail(session.Error)
			: Result<Principal?>.Ok(session.Value?.Principal);
	}

	public async Task<Result<bool>> IsSignedInAsync(CancellationToken token = default)
	{
		var session = await GetSessionAsync(token);
		return session.IsFailure
			? Result<bool>.Fail(session.Error)
			: Result<bool>.Ok(session.Value is { Status: AuthSessionStatus.Active });
	}

	public async Task<Result<AuthorizationResult>> RequireAsync(
		AuthorizationRequirement requirement,
		CancellationToken token = default
	)
	{
		var session = await GetSessionAsync(token);
		if (session.IsFailure)
			return Result<AuthorizationResult>.Fail(session.Error);

		return await _authorization.AuthorizeAsync(
			new AuthorizationRequest(session.Value?.Principal, requirement, session.Value),
			token
		);
	}

	public Task<Result<AuthorizationResult>> RequireAsync(
		PrincipalPermission permission,
		CancellationToken token = default
	) => RequireAsync(AuthorizationRequirement.Permission(permission), token);
}
