using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Stores established auth sessions without exposing provider-specific persistence details.
/// </summary>
public interface IAuthSessionStore
{
	Task<Result<AuthSession>> SaveAsync(AuthSession session, CancellationToken token = default);
	Task<Result<AuthSession?>> LoadAsync(AuthSessionId sessionId, CancellationToken token = default);
	Task<Result> DeleteAsync(AuthSessionId sessionId, CancellationToken token = default);
}
