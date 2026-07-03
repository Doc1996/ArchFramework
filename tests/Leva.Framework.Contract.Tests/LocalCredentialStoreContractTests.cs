using Leva.Framework.Fakes;
using Leva.Framework.Identity;
using Leva.Framework.Identity.Local;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class LocalCredentialStoreContractTests
{
	[Fact]
	public async Task MemoryLocalCredentialStoreContract_SaveLoadReplaceAndDelete_AreConsistent()
	{
		var store = new MemoryLocalCredentialStore();
		var credential = TestCredential("user", true);
		var disabled = credential.Disable(credential.UpdatedAt.AddMinutes(1));

		Assert.Equal(credential, ResultAssert.Success(await store.SaveAsync(credential)));
		Assert.Equal(credential, ResultAssert.Success(await store.LoadByNameAsync("user")));
		Assert.Equal(credential, ResultAssert.Success(await store.LoadByNameAsync("USER")));

		Assert.Equal(disabled, ResultAssert.Success(await store.SaveAsync(disabled)));
		Assert.Equal(disabled, ResultAssert.Success(await store.LoadByNameAsync("user")));
		ResultAssert.Success(await store.DeleteAsync("user"));
		Assert.Null(ResultAssert.Success(await store.LoadByNameAsync("user")));
	}

	[Fact]
	public async Task MemoryLocalCredentialStoreContract_DeleteMissingCredentialIsIdempotent()
	{
		var store = new MemoryLocalCredentialStore();
		ResultAssert.Success(await store.DeleteAsync("missing"));
		Assert.Null(ResultAssert.Success(await store.LoadByNameAsync("missing")));
	}

	private static LocalCredential TestCredential(string name, bool isEnabled)
	{
		var now = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
		return new LocalCredential(
			name,
			new PrincipalId("principal-1"),
			new LocalCredentialSecret("test", 1, "salt", "value"),
			isEnabled,
			now,
			now
		);
	}
}
