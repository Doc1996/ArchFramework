using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Provides the current identity session from a host-specific source.
/// </summary>
public abstract class IdentitySessionSource
{
	public abstract Task<Result<IdentitySession?>> GetSessionAsync(CancellationToken token = default);
}
