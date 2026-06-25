using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class IdentityAccessTests
{
	[Fact]
	public async Task GetSessionAsync_ReturnsCurrentSessionFromSource()
	{
		var source = new FakeIdentitySessionSource { Session = TestSession() };
		var access = new IdentityAccess(source, new AuthorizationService([], new MemoryAuditSink()));
		var result = await access.GetSessionAsync();

		Assert.Equal(source.Session, ResultAssert.Success(result));
		Assert.Equal(1, source.LoadCount);
	}

	[Fact]
	public async Task GetIdentityAsync_ReturnsIdentityFromSession()
	{
		var session = TestSession();
		var access = new IdentityAccess(
			new FakeIdentitySessionSource { Session = session },
			new AuthorizationService([], new MemoryAuditSink())
		);

		var result = await access.GetIdentityAsync();
		Assert.Equal(session.Identity, ResultAssert.Success(result));
	}

	[Fact]
	public async Task IsSignedInAsync_ReturnsTrueForActiveSession()
	{
		var access = new IdentityAccess(
			new FakeIdentitySessionSource { Session = TestSession() },
			new AuthorizationService([], new MemoryAuditSink())
		);

		var result = await access.IsSignedInAsync();
		Assert.True(ResultAssert.Success(result));
	}

	[Fact]
	public async Task RequireAsync_AuthorizesUsingCurrentSession()
	{
		var session = TestSession();
		var requirement = AuthorizationRequirement.Permission(new IdentityPermission("plans.edit"));
		var policy = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Ok(AuthorizationResult.Allowed(requirement)),
		};

		var access = new IdentityAccess(
			new FakeIdentitySessionSource { Session = session },
			new AuthorizationService([policy], new MemoryAuditSink())
		);
		var result = await access.RequireAsync(requirement);

		Assert.True(ResultAssert.Success(result).IsAuthorized);
		Assert.Equal(session, policy.LastRequest!.Session);
	}

	[Fact]
	public async Task RequireAsync_PropagatesSourceFailure()
	{
		var access = new IdentityAccess(
			new FakeIdentitySessionSource { Error = IdentityErrors.Unavailable("test") },
			new AuthorizationService([], new MemoryAuditSink())
		);
		var result = await access.RequireAsync(AuthorizationRequirement.SignedIn);

		Assert.True(result.IsFailure);
		Assert.Equal("identity.unavailable", result.Error.Code);
	}

	private static IdentitySession TestSession()
	{
		return new IdentitySession(
			new IdentitySessionId("session-1"),
			new Identity(new IdentityId("user-1"), "User One"),
			IdentitySessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);
	}
}
