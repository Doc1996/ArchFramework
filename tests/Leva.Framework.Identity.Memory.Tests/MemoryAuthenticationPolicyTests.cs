using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

public sealed class MemoryAuthenticationPolicyTests
{
	[Fact]
	public async Task AuthenticateAsync_ReturnsPrincipalForValidNameAndSecret()
	{
		var principals = new MemoryPrincipalStore();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		principals.Add(principal, "user");

		var policy = new MemoryAuthenticationPolicy(principals);
		policy.Add("user", "secret", principal.Id);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Secret, "user", "secret")
		);

		var authentication = ResultAssert.Success(result);
		Assert.True(authentication.IsAuthenticated);
		Assert.Equal(principal, authentication.Principal);
	}

	[Fact]
	public async Task AuthenticateAsync_ReturnsFailedAuthenticationForWrongSecret()
	{
		var principals = new MemoryPrincipalStore();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		principals.Add(principal, "user");

		var policy = new MemoryAuthenticationPolicy(principals);
		policy.Add("user", "secret", principal.Id);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Secret, "user", "wrong")
		);

		var authentication = ResultAssert.Success(result);
		Assert.False(authentication.IsAuthenticated);
		Assert.Equal("Principal credentials are invalid.", authentication.Reason);
	}

	[Fact]
	public async Task AuthenticationService_CreatesSessionWithMemoryPolicy()
	{
		var services = MemoryPrincipalServices.Create();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		services.Principals.Add(principal, "user");

		services.AuthenticationPolicy.Add("user", "secret", principal.Id);
		var result = await services.Authentication.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Secret, "user", "secret")
		);
		var authentication = ResultAssert.Success(result);

		Assert.True(authentication.IsAuthenticated);
		Assert.NotNull(authentication.Session);
		Assert.Equal(principal, authentication.Principal);
		Assert.Single(services.Sessions.Sessions);
	}
}
