using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable current identity session source fake for tests.
/// </summary>
public sealed class FakeIdentitySessionSource : IdentitySessionSource
{
	public IdentitySession? Session { get; set; }
	public Error? Error { get; set; }
	public int LoadCount { get; private set; }

	public override Task<Result<IdentitySession?>> GetSessionAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		LoadCount++;

		return Task.FromResult(
			Error.HasValue ? Result<IdentitySession?>.Fail(Error.Value) : Result<IdentitySession?>.Ok(Session)
		);
	}
}
