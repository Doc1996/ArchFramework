using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Stores established principal sessions without exposing provider-specific persistence details.
/// </summary>
public interface IPrincipalSessionStore
{
	Task<Result<PrincipalSession>> SaveAsync(PrincipalSession session, CancellationToken token = default);
	Task<Result<PrincipalSession?>> LoadAsync(PrincipalSessionId id, CancellationToken token = default);

	Task<Result> DeleteAsync(PrincipalSessionId id, CancellationToken token = default);
}
