using Leva.Framework.Core;

namespace Leva.Framework.Identity.AspNet.Tests;

internal sealed class EmptyAuthSessionStore : IAuthSessionStore
{
	public Task<Result<AuthSession>> SaveAsync(AuthSession session, CancellationToken token = default) =>
		Task.FromResult(Result<AuthSession>.Ok(session));

	public Task<Result<AuthSession?>> LoadAsync(AuthSessionId sessionId, CancellationToken token = default) =>
		Task.FromResult(Result<AuthSession?>.Ok(null));

	public Task<Result> DeleteAsync(AuthSessionId sessionId, CancellationToken token = default) =>
		Task.FromResult(Result.Ok());
}
