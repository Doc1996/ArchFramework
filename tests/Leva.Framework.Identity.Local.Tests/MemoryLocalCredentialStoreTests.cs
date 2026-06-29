using Xunit;

namespace Leva.Framework.Identity.Local.Tests;

public sealed class MemoryLocalCredentialStoreTests
{
	[Fact]
	public async Task SaveLoadAndDeleteAsync_StoresCredentialInMemory()
	{
		var store = new MemoryLocalCredentialStore();
		var credential = TestCredential();

		ResultAssert.Success(await store.SaveAsync(credential));
		var loaded = ResultAssert.Success(await store.LoadByNameAsync(credential.Name));
		ResultAssert.Success(await store.DeleteAsync(credential.Name));

		var deleted = ResultAssert.Success(await store.LoadByNameAsync(credential.Name));
		Assert.Equal(credential, loaded);
		Assert.Null(deleted);
	}

	[Fact]
	public async Task Clear_RemovesAllCredentials()
	{
		var store = new MemoryLocalCredentialStore();
		ResultAssert.Success(await store.SaveAsync(TestCredential()));

		store.Clear();
		Assert.Empty(store.Credentials);
	}

	private static LocalCredential TestCredential()
	{
		var utcNow = DateTimeOffset.UtcNow;
		return new LocalCredential(
			"user",
			new PrincipalId("principal-1"),
			new LocalCredentialSecret("test", 1, "salt", "value"),
			true,
			utcNow,
			utcNow
		);
	}
}
