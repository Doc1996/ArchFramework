using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

public sealed class MemoryAuthenticationPolicyTests
{
	[Fact]
	public async Task AuthenticateAsync_ReturnsPrincipalForValidName()
	{
		var principals = new MemoryPrincipalStore();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		principals.Add(principal, "user");

		var policy = new MemoryAuthenticationPolicy(principals);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Principal, "user")
		);

		var authentication = ResultAssert.Success(result);
		Assert.True(authentication.IsAuthenticated);
		Assert.Equal(principal, authentication.Principal);
	}

	[Fact]
	public async Task AuthenticateAsync_ReturnsFailedAuthenticationForWrongName()
	{
		var principals = new MemoryPrincipalStore();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		principals.Add(principal, "user");

		var policy = new MemoryAuthenticationPolicy(principals);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Principal, "wrong")
		);

		var authentication = ResultAssert.Success(result);
		Assert.False(authentication.IsAuthenticated);
		Assert.Equal("Principal name is invalid.", authentication.Reason);
	}

	[Fact]
	public async Task AuthenticationService_CreatesSessionWithMemoryPolicy()
	{
		var services = MemoryPrincipalServices.Create();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		services.Principals.Add(principal, "user");

		var result = await services.Authentication.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Principal, "user")
		);

		var authentication = ResultAssert.Success(result);
		Assert.True(authentication.IsAuthenticated);
		Assert.NotNull(authentication.Session);

		Assert.Equal(principal, authentication.Principal);
		Assert.Single(services.Sessions.Sessions);
	}
}
