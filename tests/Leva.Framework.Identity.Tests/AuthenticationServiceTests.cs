using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class AuthenticationServiceTests
{
	[Fact]
	public async Task AuthenticateAsync_UsesFirstMatchingPolicyCreatesSessionAndWritesAudit()
	{
		var method = new AuthenticationMethod("local.password");
		var unmatched = new FakeAuthenticationPolicy(new AuthenticationMethod("other"))
		{
			Result = Result<AuthenticationResult>.Ok(
				AuthenticationResult.Succeeded(new Identity(new IdentityId("bad"), "Bad"))
			),
		};

		var identity = TestIdentity();
		var matched = new FakeAuthenticationPolicy(method)
		{
			Result = Result<AuthenticationResult>.Ok(AuthenticationResult.Succeeded(identity)),
		};

		var sessionStore = new FakeIdentitySessionStore();
		var audit = new MemoryAuditSink();
		var service = new AuthenticationService(
			[unmatched, matched],
			new IdentitySessionService(sessionStore, auditSink: audit),
			audit
		);

		var result = await service.AuthenticateAsync(new AuthenticationRequest(method, "user", "secret"));
		var authentication = ResultAssert.Success(result);

		Assert.True(authentication.IsAuthenticated);
		Assert.Equal(identity, authentication.Identity);
		Assert.NotNull(authentication.Session);

		Assert.Equal(0, unmatched.AuthenticateCount);
		Assert.Equal(1, matched.AuthenticateCount);
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.Authenticated && entry.IdentityId == identity.Id
		);
	}

	[Fact]
	public async Task AuthenticateAsync_ReturnsFailureWhenNoPolicySupportsMethod()
	{
		var service = new AuthenticationService(
			[],
			new IdentitySessionService(new FakeIdentitySessionStore()),
			new MemoryAuditSink()
		);
		var result = await service.AuthenticateAsync(new AuthenticationRequest(new AuthenticationMethod("missing")));

		Assert.True(result.IsFailure);
		Assert.Equal("identity.not_found", result.Error.Code);
	}

	[Fact]
	public async Task AuthenticateAsync_WritesAuditWhenPolicyFails()
	{
		var method = new AuthenticationMethod("local.password");
		var audit = new MemoryAuditSink();
		var policy = new FakeAuthenticationPolicy(method)
		{
			Result = Result<AuthenticationResult>.Fail(IdentityErrors.Unauthorized("bad credentials")),
		};

		var service = new AuthenticationService(
			[policy],
			new IdentitySessionService(new FakeIdentitySessionStore(), auditSink: audit),
			audit
		);
		var result = await service.AuthenticateAsync(new AuthenticationRequest(method, "user", "bad"));

		Assert.True(result.IsFailure);
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.AuthenticationFailed && entry.Method == method.ToString()
		);
	}

	[Fact]
	public async Task SignOutAsync_DelegatesToSessionService()
	{
		var sessionStore = new FakeIdentitySessionStore();
		var audit = new MemoryAuditSink();
		var sessionService = new IdentitySessionService(sessionStore, auditSink: audit);

		var session = ResultAssert.Success(await sessionService.CreateAsync(TestIdentity()));
		var authentication = new AuthenticationService([], sessionService, audit);
		var result = await authentication.SignOutAsync(session.SessionId);

		ResultAssert.Success(result);
		Assert.Equal(IdentitySessionStatus.SignedOut, sessionStore.Sessions[session.SessionId].Status);
	}

	private static Identity TestIdentity() => new(new IdentityId("user-1"), "User One");
}
