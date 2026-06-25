using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Provides the current principal session from a host-specific source.
/// </summary>
public abstract class PrincipalSessionSource
{
	public abstract Task<Result<PrincipalSession?>> GetSessionAsync(CancellationToken token = default);
}
