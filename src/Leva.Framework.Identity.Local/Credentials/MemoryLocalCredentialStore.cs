using Leva.Framework.Core;

namespace Leva.Framework.Identity.Local;

/// <summary>
/// Stores local credentials in memory for tests, demos, and simple local applications.
/// </summary>
public sealed class MemoryLocalCredentialStore : ILocalCredentialStore
{
	private readonly SyncDictionary<string, LocalCredential> _credentials = new(StringComparer.OrdinalIgnoreCase);
	public IReadOnlyList<LocalCredential> Credentials => _credentials.Values();

	public Task<Result<LocalCredential>> SaveAsync(LocalCredential credential, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(credential);

		_credentials.Set(credential.Name, credential);
		return Task.FromResult(Result<LocalCredential>.Ok(credential));
	}

	public Task<Result<LocalCredential?>> LoadByNameAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Task.FromResult(Result<LocalCredential?>.Ok(_credentials.GetOrDefault(name)));
	}

	public Task<Result> DeleteAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		_credentials.Remove(name);
		return Task.FromResult(Result.Ok());
	}

	public void Clear() => _credentials.Clear();
}
