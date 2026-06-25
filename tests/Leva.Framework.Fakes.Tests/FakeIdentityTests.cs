using Leva.Framework.Core;
using Leva.Framework.Identity;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeIdentityTests
{
	[Fact]
	public async Task FakeIdentityStore_LoadsByIdAndName()
	{
		var store = new FakeIdentityStore();
		var identity = new IdentityModel(new IdentityId("user-1"), "User One");
		store.Add(identity, "user");

		var byId = await store.LoadAsync(identity.Id);
		var byName = await store.FindByNameAsync("user");

		Assert.Equal(identity, byId.Value);
		Assert.Equal(identity, byName.Value);
	}

	[Fact]
	public async Task FakeIdentitySessionStore_SavesLoadsAndDeletesSession()
	{
		var store = new FakeIdentitySessionStore();
		var session = new IdentitySession(
			new IdentitySessionId("session-1"),
			new IdentityModel(new IdentityId("user-1"), "User One"),
			IdentitySessionStatus.Active,
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
	public async Task FakeIdentitySessionSource_ReturnsConfiguredSessionOrError()
	{
		var source = new FakeIdentitySessionSource();
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

		var request = new AuthorizationRequest(new Identity(new IdentityId("user-1"), "User One"), requirement);
		var result = await policy.AuthorizeAsync(request);

		Assert.True(result.Value!.IsAuthorized);
		Assert.Equal(1, policy.AuthorizeCount);
		Assert.Equal(request, policy.LastRequest);
	}
}
