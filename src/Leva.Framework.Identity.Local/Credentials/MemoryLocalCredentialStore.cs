using Leva.Framework.Core;

namespace Leva.Framework.Identity.Local;

/// <summary>
/// Stores local credentials in memory for tests, demos, and simple local applications.
/// </summary>
public sealed class MemoryLocalCredentialStore : ILocalCredentialStore
{
	private readonly Lock _lock = new();
	private readonly Dictionary<string, LocalCredential> _credentials = new(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyList<LocalCredential> Credentials
	{
		get
		{
			lock (_lock)
				return _credentials.Values.ToList();
		}
	}

	public Task<Result<LocalCredential>> SaveAsync(LocalCredential credential, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(credential);

		lock (_lock)
			_credentials[credential.Name] = credential;
		return Task.FromResult(Result<LocalCredential>.Ok(credential));
	}

	public Task<Result<LocalCredential?>> LoadByNameAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		lock (_lock)
		{
			_credentials.TryGetValue(name, out var credential);
			return Task.FromResult(Result<LocalCredential?>.Ok(credential));
		}
	}

	public Task<Result> DeleteAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		lock (_lock)
			_credentials.Remove(name);
		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
			_credentials.Clear();
	}
}
