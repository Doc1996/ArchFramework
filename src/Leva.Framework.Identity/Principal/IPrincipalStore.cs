using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Loads principals from provider-specific stores or account sources.
/// </summary>
public interface IPrincipalStore
{
	Task<Result<Principal?>> LoadAsync(PrincipalId id, CancellationToken token = default);
	Task<Result<Principal?>> FindByNameAsync(string name, CancellationToken token = default);
}
