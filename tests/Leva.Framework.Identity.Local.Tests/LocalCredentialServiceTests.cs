using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Local.Tests;

public sealed class LocalCredentialServiceTests
{
	[Fact]
	public async Task CreateAsync_ProtectsAndStoresCredential()
	{
		var store = new MemoryLocalCredentialStore();
		var service = new LocalCredentialService(store, new LocalSecretProtector(1_000, 16, 32));
		var credential = ResultAssert.Success(
			await service.CreateAsync("user", "secret", new PrincipalId("principal-1"))
		);

		var loaded = ResultAssert.Success(await store.LoadByNameAsync("user"));
		Assert.Equal("user", credential.Name);
		Assert.Equal(credential, loaded);

		Assert.True(credential.IsEnabled);
		Assert.NotEqual("secret", credential.Secret.Value);
	}

	[Fact]
	public async Task ChangeSecretAsync_UpdatesStoredSecret()
	{
		var store = new MemoryLocalCredentialStore();
		var service = new LocalCredentialService(store, new LocalSecretProtector(1_000, 16, 32));

		var created = ResultAssert.Success(await service.CreateAsync("user", "secret", new PrincipalId("principal-1")));
		var changed = ResultAssert.Success(await service.ChangeSecretAsync("user", "new-secret"));
		Assert.NotEqual(created.Secret.Value, changed.Secret.Value);
	}

	[Fact]
	public async Task DisableAndEnableAsync_UpdatesCredentialState()
	{
		var store = new MemoryLocalCredentialStore();
		var service = new LocalCredentialService(store, new LocalSecretProtector(1_000, 16, 32));

		ResultAssert.Success(await service.CreateAsync("user", "secret", new PrincipalId("principal-1")));
		var disabled = ResultAssert.Success(await service.DisableAsync("user"));
		var enabled = ResultAssert.Success(await service.EnableAsync("user"));

		Assert.False(disabled.IsEnabled);
		Assert.True(enabled.IsEnabled);
	}

	[Fact]
	public async Task DeleteAsync_RemovesCredential()
	{
		var store = new MemoryLocalCredentialStore();
		var service = new LocalCredentialService(store, new LocalSecretProtector(1_000, 16, 32));

		ResultAssert.Success(await service.CreateAsync("user", "secret", new PrincipalId("principal-1")));
		ResultAssert.Success(await service.DeleteAsync("user"));
		var loaded = ResultAssert.Success(await store.LoadByNameAsync("user"));
		Assert.Null(loaded);
	}
}
