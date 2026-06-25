using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Loads identities from provider-specific storage or account sources.
/// </summary>
public abstract class IdentityStore
{
	public abstract Task<Result<Identity?>> LoadAsync(IdentityId identityId, CancellationToken token = default);

	public abstract Task<Result<Identity?>> FindByNameAsync(string name, CancellationToken token = default);
}
