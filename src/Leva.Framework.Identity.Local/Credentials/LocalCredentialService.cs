using Leva.Framework.Core;

namespace Leva.Framework.Identity.Local;

/// <summary>
/// Creates, updates, enables, disables, and deletes local credentials.
/// </summary>
public sealed class LocalCredentialService
{
	private readonly ILocalCredentialStore _credentials;
	private readonly LocalSecretProtector _secretProtector;
	private readonly IClock? _clock;

	public LocalCredentialService(
		ILocalCredentialStore credentials,
		LocalSecretProtector? secretProtector = null,
		IClock? clock = null
	)
	{
		ArgumentNullException.ThrowIfNull(credentials);

		_credentials = credentials;
		_secretProtector = secretProtector ?? new LocalSecretProtector();
		_clock = clock;
	}

	public async Task<Result<LocalCredential>> CreateAsync(
		string name,
		string secret,
		PrincipalId principalId,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(secret);

		var utcNow = UtcNow;
		var credential = new LocalCredential(name, principalId, _secretProtector.Protect(secret), true, utcNow, utcNow);

		return await _credentials.SaveAsync(credential, token);
	}

	public async Task<Result<LocalCredential>> ChangeSecretAsync(
		string name,
		string secret,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(secret);

		var credential = await LoadRequiredAsync(name, token);
		return credential.IsFailure
			? Result<LocalCredential>.Fail(credential.Error)
			: await _credentials.SaveAsync(
				credential.Value!.ChangeSecret(_secretProtector.Protect(secret), UtcNow),
				token
			);
	}

	public Task<Result<LocalCredential>> EnableAsync(string name, CancellationToken token = default) =>
		SetEnabledAsync(name, true, token);

	public Task<Result<LocalCredential>> DisableAsync(string name, CancellationToken token = default) =>
		SetEnabledAsync(name, false, token);

	public Task<Result> DeleteAsync(string name, CancellationToken token = default) =>
		_credentials.DeleteAsync(name, token);

	private async Task<Result<LocalCredential>> SetEnabledAsync(string name, bool isEnabled, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		var credential = await LoadRequiredAsync(name, token);
		if (credential.IsFailure)
			return Result<LocalCredential>.Fail(credential.Error);

		var updated = isEnabled ? credential.Value!.Enable(UtcNow) : credential.Value!.Disable(UtcNow);
		return await _credentials.SaveAsync(updated, token);
	}

	private async Task<Result<LocalCredential>> LoadRequiredAsync(string name, CancellationToken token)
	{
		var result = await _credentials.LoadByNameAsync(name, token);
		if (result.IsFailure)
			return Result<LocalCredential>.Fail(result.Error);
		return result.Value is null
			? Result<LocalCredential>.Fail(PrincipalErrors.NotFound("local credential", name))
			: Result<LocalCredential>.Ok(result.Value);
	}

	private DateTimeOffset UtcNow => _clock?.UtcNow ?? DateTimeOffset.UtcNow;
}
