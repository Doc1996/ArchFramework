using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Stores established identity sessions without exposing provider-specific persistence details.
/// </summary>
public abstract class IdentitySessionStore
{
	public abstract Task<Result<IdentitySession>> SaveAsync(IdentitySession session, CancellationToken token = default);

	public abstract Task<Result<IdentitySession?>> LoadAsync(
		IdentitySessionId sessionId,
		CancellationToken token = default
	);

	public abstract Task<Result> DeleteAsync(IdentitySessionId sessionId, CancellationToken token = default);
}
