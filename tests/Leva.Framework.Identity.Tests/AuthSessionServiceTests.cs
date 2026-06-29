using Leva.Framework.Fakes;
using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class AuthSessionServiceTests
{
	[Fact]
	public async Task CreateAsync_SavesActiveSessionAndWritesAudit()
	{
		var store = new FakeAuthSessionStore();
		var audit = new MemoryAuditSink();
		var service = new AuthSessionService(store, auditSink: audit);

		var principal = TestPrincipal();
		var result = await service.CreateAsync(principal);
		var session = ResultAssert.Success(result);

		Assert.True(session.IsActive);
		Assert.Equal(principal, session.Principal);
		Assert.True(store.Sessions.ContainsKey(session.SessionId));

		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.SessionCreated && entry.SessionId == session.SessionId
		);
	}

	[Fact]
	public async Task LoadAsync_ExpiresExpiredSessionAndReturnsNull()
	{
		var store = new FakeAuthSessionStore();
		var audit = new MemoryAuditSink();
		var service = new AuthSessionService(store, new AuthSessionPolicy(TimeSpan.FromMilliseconds(-1)), audit);

		var session = ResultAssert.Success(await service.CreateAsync(TestPrincipal()));
		var result = await service.LoadAsync(session.SessionId);

		Assert.True(result.IsSuccess);
		Assert.Null(result.Value);
		Assert.Equal(AuthSessionStatus.Expired, store.Sessions[session.SessionId].SessionStatus);
		Assert.Contains(audit.AuditEntries, entry => entry.Action == AuditAction.SessionExpired);
	}

	[Fact]
	public async Task SignOutAsync_MarksSessionSignedOutAndWritesAudit()
	{
		var store = new FakeAuthSessionStore();
		var audit = new MemoryAuditSink();
		var service = new AuthSessionService(store, auditSink: audit);

		var session = ResultAssert.Success(await service.CreateAsync(TestPrincipal()));
		var result = await service.SignOutAsync(session.SessionId);
		var signedOut = ResultAssert.Success(result);

		Assert.Equal(AuthSessionStatus.SignedOut, signedOut.SessionStatus);
		Assert.False(signedOut.IsActive);
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.SignedOut && entry.SessionId == session.SessionId
		);
	}

	[Fact]
	public async Task DeleteAsync_DeletesSessionAndWritesAudit()
	{
		var store = new FakeAuthSessionStore();
		var audit = new MemoryAuditSink();
		var service = new AuthSessionService(store, auditSink: audit);

		var session = ResultAssert.Success(await service.CreateAsync(TestPrincipal()));
		var result = await service.DeleteAsync(session.SessionId);

		ResultAssert.Success(result);
		Assert.False(store.Sessions.ContainsKey(session.SessionId));
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.SessionDeleted && entry.SessionId == session.SessionId
		);
	}

	private static Principal TestPrincipal() => new(new PrincipalId("user-1"), "User One");
}
