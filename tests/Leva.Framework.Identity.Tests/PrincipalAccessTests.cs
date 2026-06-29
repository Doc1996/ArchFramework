using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class PrincipalAccessTests
{
	[Fact]
	public async Task GetSessionAsync_ReturnsCurrentSessionFromSource()
	{
		var source = new FakePrincipalSessionSource { Session = TestSession() };
		var access = new PrincipalAccess(source, new AuthorizationService([], new MemoryAuditSink()));
		var result = await access.GetSessionAsync();

		Assert.Equal(source.Session, ResultAssert.Success(result));
		Assert.Equal(1, source.LoadCount);
	}

	[Fact]
	public async Task GetPrincipalAsync_ReturnsPrincipalFromSession()
	{
		var session = TestSession();
		var access = new PrincipalAccess(
			new FakePrincipalSessionSource { Session = session },
			new AuthorizationService([], new MemoryAuditSink())
		);

		var result = await access.GetPrincipalAsync();
		Assert.Equal(session.Principal, ResultAssert.Success(result));
	}

	[Fact]
	public async Task IsSignedInAsync_ReturnsTrueForActiveSession()
	{
		var access = new PrincipalAccess(
			new FakePrincipalSessionSource { Session = TestSession() },
			new AuthorizationService([], new MemoryAuditSink())
		);

		var result = await access.IsSignedInAsync();
		Assert.True(ResultAssert.Success(result));
	}

	[Fact]
	public async Task RequireAsync_AuthorizesUsingCurrentSession()
	{
		var session = TestSession();
		var requirement = AuthorizationRequirement.Permission(new PrincipalPermission("plans.edit"));
		var policy = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Ok(AuthorizationResult.Allowed(requirement)),
		};

		var access = new PrincipalAccess(
			new FakePrincipalSessionSource { Session = session },
			new AuthorizationService([policy], new MemoryAuditSink())
		);
		var result = await access.RequireAsync(requirement);

		Assert.True(ResultAssert.Success(result).IsAuthorized);
		Assert.Equal(session, policy.LastRequest!.Session);
	}

	[Fact]
	public async Task RequireAsync_PropagatesSourceFailure()
	{
		var access = new PrincipalAccess(
			new FakePrincipalSessionSource { Error = PrincipalErrors.Unavailable("test") },
			new AuthorizationService([], new MemoryAuditSink())
		);
		var result = await access.RequireAsync(AuthorizationRequirement.SignedIn);

		Assert.True(result.IsFailure);
		Assert.Equal("principal.unavailable", result.Error.Code);
	}

	private static PrincipalSession TestSession()
	{
		return new PrincipalSession(
			new PrincipalSessionId("session-1"),
			new Principal(new PrincipalId("user-1"), "User One"),
			PrincipalSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);
	}
}
