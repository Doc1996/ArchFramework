using Leva.Framework.Core;
using Leva.Framework.Identity;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakePrincipalTests
{
	[Fact]
	public async Task FakePrincipalStore_LoadsByIdAndName()
	{
		var store = new FakePrincipalStore();
		var principal = new Principal(new PrincipalId("user-1"), "User One");
		store.Add(principal, "user");

		var byId = await store.LoadAsync(principal.Id);
		var byName = await store.FindByNameAsync("user");

		Assert.Equal(principal, byId.Value);
		Assert.Equal(principal, byName.Value);
	}

	[Fact]
	public async Task FakePrincipalSessionStore_SavesLoadsAndDeletesSession()
	{
		var store = new FakePrincipalSessionStore();
		var session = new PrincipalSession(
			new PrincipalSessionId("session-1"),
			new Principal(new PrincipalId("user-1"), "User One"),
			PrincipalSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);

		await store.SaveAsync(session);
		var loaded = await store.LoadAsync(session.SessionId);
		await store.DeleteAsync(session.SessionId);
		var deleted = await store.LoadAsync(session.SessionId);

		Assert.Equal(session, loaded.Value);
		Assert.Null(deleted.Value);
	}

	[Fact]
	public async Task FakePrincipalSessionSource_ReturnsConfiguredSessionOrError()
	{
		var source = new FakePrincipalSessionSource();
		var error = new Error("test", "failure");
		source.Error = error;
		var result = await source.GetSessionAsync();

		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
		Assert.Equal(1, source.LoadCount);
	}

	[Fact]
	public async Task FakeAuthenticationPolicy_RecordsCalls()
	{
		var method = new AuthenticationMethod("test");
		var policy = new FakeAuthenticationPolicy(method)
		{
			Result = Result<AuthenticationResult>.Ok(AuthenticationResult.Failed("no")),
		};

		var request = new AuthenticationRequest(method);
		var result = await policy.AuthenticateAsync(request);

		Assert.True(result.IsSuccess);
		Assert.Equal(1, policy.AuthenticateCount);
		Assert.Equal(request, policy.LastRequest);
	}

	[Fact]
	public async Task FakeAuthorizationPolicy_RecordsCalls()
	{
		var requirement = AuthorizationRequirement.SignedIn;
		var policy = new FakeAuthorizationPolicy
		{
			Result = Result<AuthorizationResult>.Ok(AuthorizationResult.Allowed(requirement)),
		};

		var request = new AuthorizationRequest(new Principal(new PrincipalId("user-1"), "User One"), requirement);
		var result = await policy.AuthorizeAsync(request);

		Assert.True(result.Value!.IsAuthorized);
		Assert.Equal(1, policy.AuthorizeCount);
		Assert.Equal(request, policy.LastRequest);
	}
}
