using Xunit;

namespace Leva.Framework.Identity.Local.Tests;

public sealed class LocalSecretProtectorTests
{
	[Fact]
	public void Protect_CreatesVerifiableSecret()
	{
		var protector = new LocalSecretProtector(1_000, 16, 32);
		var secret = protector.Protect("secret");

		Assert.Equal("PBKDF2-SHA256", secret.Algorithm);
		Assert.True(protector.Verify("secret", secret));
		Assert.False(protector.Verify("wrong", secret));
	}

	[Fact]
	public void Protect_UsesUniqueSalt()
	{
		var protector = new LocalSecretProtector(1_000, 16, 32);
		var first = protector.Protect("secret");
		var second = protector.Protect("secret");

		Assert.NotEqual(first.Salt, second.Salt);
		Assert.NotEqual(first.Value, second.Value);
	}
}
