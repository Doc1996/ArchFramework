using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Stores established identity sessions without exposing provider-specific persistence details.
/// </summary>
public interface IIdentitySessionStore
{
	Task<Result<IdentitySession>> SaveAsync(IdentitySession session, CancellationToken token = default);
	Task<Result<IdentitySession?>> LoadAsync(IdentitySessionId id, CancellationToken token = default);

	Task<Result> DeleteAsync(IdentitySessionId id, CancellationToken token = default);
}
