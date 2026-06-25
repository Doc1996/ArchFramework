using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// State-facing principal capability that resolves the current session and performs authorization checks.
/// </summary>
public sealed class PrincipalAccess
{
	private readonly PrincipalSessionSource _sessionSource;
	private readonly AuthorizationService _authorization;

	public PrincipalAccess(PrincipalSessionSource sessionSource, AuthorizationService authorization)
	{
		ArgumentNullException.ThrowIfNull(sessionSource);
		ArgumentNullException.ThrowIfNull(authorization);

		_sessionSource = sessionSource;
		_authorization = authorization;
	}

	public Task<Result<PrincipalSession?>> GetSessionAsync(CancellationToken token = default) =>
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
			: Result<bool>.Ok(session.Value is { Status: PrincipalSessionStatus.Active });
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
