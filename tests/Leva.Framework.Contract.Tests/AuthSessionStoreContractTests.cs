using Leva.Framework.Fakes;
using Leva.Framework.Identity;
using Leva.Framework.Identity.Memory;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class AuthSessionStoreContractTests
{
	[Theory]
	[InlineData("memory")]
	[InlineData("fake")]
	public async Task AuthSessionStoreContract_SaveLoadReplaceAndDelete_AreConsistent(string storeName)
	{
		var store = CreateStore(storeName);
		var session = TestSession(storeName, AuthSessionStatus.Active);
		var signedOut = session with
		{
			SessionStatus = AuthSessionStatus.SignedOut,
			UpdatedAt = session.UpdatedAt.AddMinutes(5),
		};

		Assert.Equal(session, ResultAssert.Success(await store.SaveAsync(session)));
		Assert.Equal(session, ResultAssert.Success(await store.LoadAsync(session.SessionId)));

		Assert.Equal(signedOut, ResultAssert.Success(await store.SaveAsync(signedOut)));
		Assert.Equal(signedOut, ResultAssert.Success(await store.LoadAsync(session.SessionId)));
		ResultAssert.Success(await store.DeleteAsync(session.SessionId));
		Assert.Null(ResultAssert.Success(await store.LoadAsync(session.SessionId)));
	}

	[Theory]
	[InlineData("memory")]
	[InlineData("fake")]
	public async Task AuthSessionStoreContract_DeleteMissingSessionIsIdempotent(string storeName)
	{
		var store = CreateStore(storeName);
		ResultAssert.Success(await store.DeleteAsync(new AuthSessionId("missing-" + storeName)));
		Assert.Null(ResultAssert.Success(await store.LoadAsync(new AuthSessionId("missing-" + storeName))));
	}

	private static IAuthSessionStore CreateStore(string storeName) =>
		storeName switch
		{
			"memory" => new MemoryAuthSessionStore(),
			"fake" => new FakeAuthSessionStore(),
			_ => throw new ArgumentOutOfRangeException(nameof(storeName), storeName, "Unknown auth session store."),
		};

	private static AuthSession TestSession(string storeName, AuthSessionStatus status)
	{
		var now = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
		var principal = new Principal(new PrincipalId("principal-" + storeName), "User One");
		return new AuthSession(new AuthSessionId("session-" + storeName), principal, status, now, now, now.AddHours(1));
	}
}
