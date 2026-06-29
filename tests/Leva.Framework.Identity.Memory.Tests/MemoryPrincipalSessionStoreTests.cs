using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

public sealed class MemoryPrincipalSessionStoreTests
{
	[Fact]
	public async Task SaveLoadAndDeleteAsync_StoresSessionInMemory()
	{
		var store = new MemoryPrincipalSessionStore();
		var session = TestSession();

		ResultAssert.Success(await store.SaveAsync(session));
		var loaded = ResultAssert.Success(await store.LoadAsync(session.SessionId));
		ResultAssert.Success(await store.DeleteAsync(session.SessionId));
		var deleted = ResultAssert.Success(await store.LoadAsync(session.SessionId));

		Assert.Equal(session, loaded);
		Assert.Null(deleted);
	}

	[Fact]
	public async Task Clear_RemovesAllSessions()
	{
		var store = new MemoryPrincipalSessionStore();
		ResultAssert.Success(await store.SaveAsync(TestSession()));

		store.Clear();
		Assert.Empty(store.Sessions);
	}

	private static PrincipalSession TestSession()
	{
		return new(
			new PrincipalSessionId("session-1"),
			new Principal(new PrincipalId("principal-1"), "User One"),
			PrincipalSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);
	}
}
