using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Loads identities from provider-specific stores or account sources.
/// </summary>
public interface IIdentityStore
{
	Task<Result<Identity?>> LoadAsync(IdentityId id, CancellationToken token = default);
	Task<Result<Identity?>> FindByNameAsync(string name, CancellationToken token = default);
}
