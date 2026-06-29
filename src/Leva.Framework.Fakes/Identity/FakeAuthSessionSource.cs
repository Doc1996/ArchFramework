using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable current auth session source fake for tests.
/// </summary>
public sealed class FakeAuthSessionSource : AuthSessionSource
{
	public AuthSession? Session { get; set; }
	public Error? Error { get; set; }
	public int LoadCount { get; private set; }

	public override Task<Result<AuthSession?>> GetSessionAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		LoadCount++;

		return Task.FromResult(
			Error.HasValue ? Result<AuthSession?>.Fail(Error.Value) : Result<AuthSession?>.Ok(Session)
		);
	}
}
