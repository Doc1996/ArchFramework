using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable current principal session source fake for tests.
/// </summary>
public sealed class FakePrincipalSessionSource : PrincipalSessionSource
{
	public PrincipalSession? Session { get; set; }
	public Error? Error { get; set; }
	public int LoadCount { get; private set; }

	public override Task<Result<PrincipalSession?>> GetSessionAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		LoadCount++;

		return Task.FromResult(
			Error.HasValue ? Result<PrincipalSession?>.Fail(Error.Value) : Result<PrincipalSession?>.Ok(Session)
		);
	}
}
