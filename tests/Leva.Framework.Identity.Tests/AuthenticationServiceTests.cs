using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class AuthenticationServiceTests
{
	[Fact]
	public async Task AuthenticateAsync_UsesFirstMatchingPolicyCreatesSessionAndWritesAudit()
	{
		var method = new AuthenticationMethod("local.secret");
		var unmatched = new FakeAuthenticationPolicy(new AuthenticationMethod("other"))
		{
			Result = Result<AuthenticationResult>.Ok(
				AuthenticationResult.Succeeded(new Principal(new PrincipalId("bad"), "Bad"))
			),
		};

		var principal = TestPrincipal();
		var matched = new FakeAuthenticationPolicy(method)
		{
			Result = Result<AuthenticationResult>.Ok(AuthenticationResult.Succeeded(principal)),
		};

		var sessionStore = new FakeAuthSessionStore();
		var audit = new MemoryAuditSink();
		var service = new AuthenticationService(
			[unmatched, matched],
			new AuthSessionService(sessionStore, auditSink: audit),
			audit
		);

		var result = await service.AuthenticateAsync(new AuthenticationRequest(method, "user", "secret"));
		var authentication = ResultAssert.Success(result);

		Assert.True(authentication.IsAuthenticated);
		Assert.Equal(principal, authentication.Principal);
		Assert.NotNull(authentication.Session);

		Assert.Equal(0, unmatched.AuthenticateCount);
		Assert.Equal(1, matched.AuthenticateCount);
		Assert.Contains(
			audit.AuditEntries,
			entry => entry.Action == AuditAction.Authenticated && entry.PrincipalId == principal.Id
		);
	}

	[Fact]
	public async Task AuthenticateAsync_ReturnsFailureWhenNoPolicySupportsMethod()
	{
		var service = new AuthenticationService(
			[],
			new AuthSessionService(new FakeAuthSessionStore()),
			new MemoryAuditSink()
		);
		var result = await service.AuthenticateAsync(new AuthenticationRequest(new AuthenticationMethod("missing")));

		Assert.True(result.IsFailure);
		Assert.Equal("principal.not_found", result.Error.Code);
	}

	[Fact]
	public async Task AuthenticateAsync_WritesAuditWhenPolicyFails()
	{
		var method = new AuthenticationMethod("local.secret");
		var audit = new MemoryAuditSink();
		var policy = new FakeAuthenticationPolicy(method)
		{
			Result = Result<AuthenticationResult>.Fail(PrincipalErrors.Unauthorized("bad credentials")),
		};

		var service = new AuthenticationService(
			[policy],
			new AuthSessionService(new FakeAuthSessionStore(), auditSink: audit),
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
		var sessionStore = new FakeAuthSessionStore();
		var audit = new MemoryAuditSink();
		var sessionService = new AuthSessionService(sessionStore, auditSink: audit);

		var session = ResultAssert.Success(await sessionService.CreateAsync(TestPrincipal()));
		var authentication = new AuthenticationService([], sessionService, audit);
		var result = await authentication.SignOutAsync(session.SessionId);

		ResultAssert.Success(result);
		Assert.Equal(AuthSessionStatus.SignedOut, sessionStore.Sessions[session.SessionId].SessionStatus);
	}

	private static Principal TestPrincipal() => new(new PrincipalId("user-1"), "User One");
}
