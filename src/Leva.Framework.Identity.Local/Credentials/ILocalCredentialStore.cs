using Leva.Framework.Core;

namespace Leva.Framework.Identity.Local;

/// <summary>
/// Stores local credentials used by local authentication policies.
/// </summary>
public interface ILocalCredentialStore
{
	Task<Result<LocalCredential>> SaveAsync(LocalCredential credential, CancellationToken token = default);
	Task<Result<LocalCredential?>> LoadByNameAsync(string name, CancellationToken token = default);
	Task<Result> DeleteAsync(string name, CancellationToken token = default);
}
