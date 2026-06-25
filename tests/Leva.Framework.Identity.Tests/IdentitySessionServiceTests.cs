using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class IdentitySessionServiceTests
{
	[Fact]
	public async Task CreateAsync_SavesActiveSessionAndWritesAudit()
	{
		var store = new FakeIdentitySessionStore();
		var audit = new MemoryAuditSink();
		var service = new IdentitySessionService(store, auditSink: audit);

		var identity = TestIdentity();
		var result = await service.CreateAsync(identity);
		var session = ResultAssert.Success(result);

		Assert.True(session.IsActive);
		Assert.Equal(identity, session.Identity);
		Assert.True(store.Sessions.ContainsKey(session.SessionId));

		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.SessionCreated && entry.SessionId == session.SessionId
		);
	}

	[Fact]
	public async Task LoadAsync_ExpiresExpiredSessionAndReturnsNull()
	{
		var store = new FakeIdentitySessionStore();
		var audit = new MemoryAuditSink();
		var service = new IdentitySessionService(
			store,
			new IdentitySessionPolicy(TimeSpan.FromMilliseconds(-1)),
			audit
		);

		var session = ResultAssert.Success(await service.CreateAsync(TestIdentity()));
		var result = await service.LoadAsync(session.SessionId);

		Assert.True(result.IsSuccess);
		Assert.Null(result.Value);
		Assert.Equal(IdentitySessionStatus.Expired, store.Sessions[session.SessionId].Status);
		Assert.Contains(audit.AuditEntries, entry => entry.Action == AuditAction.SessionExpired);
	}

	[Fact]
	public async Task SignOutAsync_MarksSessionSignedOutAndWritesAudit()
	{
		var store = new FakeIdentitySessionStore();
		var audit = new MemoryAuditSink();
		var service = new IdentitySessionService(store, auditSink: audit);

		var session = ResultAssert.Success(await service.CreateAsync(TestIdentity()));
		var result = await service.SignOutAsync(session.SessionId);
		var signedOut = ResultAssert.Success(result);

		Assert.Equal(IdentitySessionStatus.SignedOut, signedOut.Status);
		Assert.False(signedOut.IsActive);
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.SignedOut && entry.SessionId == session.SessionId
		);
	}

	[Fact]
	public async Task DeleteAsync_DeletesSessionAndWritesAudit()
	{
		var store = new FakeIdentitySessionStore();
		var audit = new MemoryAuditSink();
		var service = new IdentitySessionService(store, auditSink: audit);

		var session = ResultAssert.Success(await service.CreateAsync(TestIdentity()));
		var result = await service.DeleteAsync(session.SessionId);

		ResultAssert.Success(result);
		Assert.False(store.Sessions.ContainsKey(session.SessionId));
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.SessionDeleted && entry.SessionId == session.SessionId
		);
	}

	private static Identity TestIdentity() => new(new IdentityId("user-1"), "User One");
}
