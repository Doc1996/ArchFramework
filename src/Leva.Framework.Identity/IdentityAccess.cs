using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// State-facing identity capability that resolves the current session and performs authorization checks.
/// </summary>
public sealed class IdentityAccess
{
	private readonly IdentitySessionSource _sessionSource;
	private readonly AuthorizationService _authorization;

	public IdentityAccess(IdentitySessionSource sessionSource, AuthorizationService authorization)
	{
		ArgumentNullException.ThrowIfNull(sessionSource);
		ArgumentNullException.ThrowIfNull(authorization);

		_sessionSource = sessionSource;
		_authorization = authorization;
	}

	public Task<Result<IdentitySession?>> GetSessionAsync(CancellationToken token = default) =>
		_sessionSource.GetSessionAsync(token);

	public async Task<Result<Identity?>> GetIdentityAsync(CancellationToken token = default)
	{
		var session = await GetSessionAsync(token);
		return session.IsFailure
			? Result<Identity?>.Fail(session.Error)
			: Result<Identity?>.Ok(session.Value?.Identity);
	}

	public async Task<Result<bool>> IsSignedInAsync(CancellationToken token = default)
	{
		var session = await GetSessionAsync(token);
		return session.IsFailure
			? Result<bool>.Fail(session.Error)
			: Result<bool>.Ok(session.Value is { Status: IdentitySessionStatus.Active });
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
			new AuthorizationRequest(session.Value?.Identity, requirement, session.Value),
			token
		);
	}

	public Task<Result<AuthorizationResult>> RequireAsync(
		IdentityPermission permission,
		CancellationToken token = default
	) => RequireAsync(AuthorizationRequirement.Permission(permission), token);
}
