using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Resolves the current identity session from a host-specific execution context.
/// </summary>
public abstract class IdentitySessionResolver
{
	public abstract Task<Result<IdentitySession?>> GetSessionAsync(CancellationToken token = default);
}
