using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Provides the current auth session from a host-specific source.
/// </summary>
public abstract class AuthSessionSource
{
	public abstract Task<Result<AuthSession?>> GetSessionAsync(CancellationToken token = default);
}
