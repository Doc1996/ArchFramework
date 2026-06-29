using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

public sealed class MemoryPrincipalStoreTests
{
	[Fact]
	public async Task LoadAsync_LoadsPrincipalById()
	{
		var store = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		store.Add(principal, "user");

		var loaded = ResultAssert.Success(await store.LoadAsync(principal.Id));
		Assert.Equal(principal, loaded);
	}

	[Fact]
	public async Task FindByNameAsync_LoadsPrincipalByExplicitNameDisplayNameAndEmail()
	{
		var store = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		store.Add(principal, "user");

		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("user")));
		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("User One")));
		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("user@example.com")));
	}

	[Fact]
	public async Task Remove_RemovesPrincipalAndNames()
	{
		var store = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		store.Add(principal, "user");

		var removed = store.Remove(principal.Id);
		var byId = ResultAssert.Success(await store.LoadAsync(principal.Id));
		var byName = ResultAssert.Success(await store.FindByNameAsync("user"));

		Assert.True(removed);
		Assert.Null(byId);
		Assert.Null(byName);
	}

	[Fact]
	public void Clear_RemovesAllPrincipals()
	{
		var store = new MemoryPrincipalStore();
		store.Add(TestPrincipal(), "user");

		store.Clear();
		Assert.Empty(store.Principals);
	}

	private static Principal TestPrincipal() => new(new PrincipalId("principal-1"), "User One", "user@example.com");
}
