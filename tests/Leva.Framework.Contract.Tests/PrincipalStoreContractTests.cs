using Leva.Framework.Fakes;
using Leva.Framework.Identity;
using Leva.Framework.Identity.Memory;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class PrincipalStoreContractTests
{
	[Fact]
	public async Task MemoryPrincipalStoreContract_LoadsByIdAndKnownNames()
	{
		var store = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		store.Add(principal, "user", "operator");

		Assert.Equal(principal, ResultAssert.Success(await store.LoadAsync(principal.Id)));
		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("user")));
		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("operator")));

		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("User One")));
		Assert.Equal(principal, ResultAssert.Success(await store.FindByNameAsync("user@example.com")));
	}

	[Fact]
	public async Task MemoryPrincipalStoreContract_ReturnsNullForMissingPrincipal()
	{
		var store = new MemoryPrincipalStore();
		Assert.Null(ResultAssert.Success(await store.LoadAsync(new PrincipalId("missing"))));
		Assert.Null(ResultAssert.Success(await store.FindByNameAsync("missing")));
	}

	[Fact]
	public async Task MemoryPrincipalStoreContract_RemoveDeletesPrincipalAndAllNames()
	{
		var store = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		store.Add(principal, "user", "operator");

		Assert.True(store.Remove(principal.Id));
		Assert.Null(ResultAssert.Success(await store.LoadAsync(principal.Id)));
		Assert.Null(ResultAssert.Success(await store.FindByNameAsync("user")));

		Assert.Null(ResultAssert.Success(await store.FindByNameAsync("operator")));
		Assert.Null(ResultAssert.Success(await store.FindByNameAsync("User One")));
		Assert.Null(ResultAssert.Success(await store.FindByNameAsync("user@example.com")));
	}

	private static Principal TestPrincipal() =>
		new(
			new PrincipalId("principal-1"),
			"User One",
			"user@example.com",
			new HashSet<PrincipalRole> { new("operator") },
			new HashSet<PrincipalPermission> { new("tickets.manage") }
		);
}
