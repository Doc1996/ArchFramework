using Leva.Framework.Core;

namespace Leva.Framework.Identity.Local;

/// <summary>
/// Authenticates principals with local name and secret credentials.
/// </summary>
public sealed class LocalAuthenticationPolicy : AuthenticationPolicy
{
	private readonly ILocalCredentialStore _credentials;
	private readonly IPrincipalStore _principals;
	private readonly LocalSecretProtector _secretProtector;

	public LocalAuthenticationPolicy(
		ILocalCredentialStore credentials,
		IPrincipalStore principals,
		LocalSecretProtector? secretProtector = null,
		AuthenticationMethod? method = null
	)
	{
		ArgumentNullException.ThrowIfNull(credentials);
		ArgumentNullException.ThrowIfNull(principals);

		_credentials = credentials;
		_principals = principals;
		_secretProtector = secretProtector ?? new LocalSecretProtector();
		Method = method ?? LocalAuthenticationMethods.Secret;
	}

	public override AuthenticationMethod Method { get; }

	public override async Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		if (string.IsNullOrWhiteSpace(request.Name))
			return Succeeded(AuthenticationResult.Failed("Principal name is required."));

		if (string.IsNullOrWhiteSpace(request.Secret))
			return Succeeded(AuthenticationResult.Failed("Principal secret is required."));

		var credential = await _credentials.LoadByNameAsync(request.Name, token);
		if (credential.IsFailure)
			return Result<AuthenticationResult>.Fail(credential.Error);

		if (credential.Value is null || !credential.Value.IsEnabled)
			return InvalidCredentials();

		if (!_secretProtector.Verify(request.Secret, credential.Value.Secret))
			return InvalidCredentials();

		var principal = await _principals.LoadAsync(credential.Value.PrincipalId, token);
		if (principal.IsFailure)
			return Result<AuthenticationResult>.Fail(principal.Error);

		return principal.Value is null
			? Succeeded(AuthenticationResult.Failed("Principal is not available."))
			: Succeeded(AuthenticationResult.Succeeded(principal.Value));
	}

	private static Result<AuthenticationResult> InvalidCredentials() =>
		Succeeded(AuthenticationResult.Failed("Principal credentials are invalid."));

	private static Result<AuthenticationResult> Succeeded(AuthenticationResult result) =>
		Result<AuthenticationResult>.Ok(result);
}
