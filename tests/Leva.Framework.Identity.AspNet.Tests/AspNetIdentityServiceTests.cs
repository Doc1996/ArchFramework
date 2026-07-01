using Leva.Framework.Fakes;
using Leva.Framework.Identity.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetIdentityServiceTests
{
	[Fact]
	public async Task LoginAsync_AuthenticatesAndWritesSessionCookie()
	{
		var memory = MemoryPrincipalServices.Create();
		memory.Principals.Add(new Principal(new PrincipalId("principal-1"), "User One"), "user");
		var options = Options.Create(new AspNetIdentityOptions { SecureCookie = false });

		var service = new AspNetIdentityService(
			memory.Authentication,
			memory.SessionService,
			new AspNetAuthSessionReader(options),
			new AspNetAuthSessionWriter(options)
		);

		var context = new DefaultHttpContext();
		var result = await service.LoginAsync(
			context,
			new AuthenticationRequest(MemoryAuthenticationMethods.Principal, "user")
		);

		var authentication = ResultAssert.Success(result);
		Assert.True(authentication.IsAuthenticated);
		Assert.NotNull(authentication.Session);
		Assert.Contains(".Framework.AuthSession=", context.Response.Headers.SetCookie.ToString());
	}

	[Fact]
	public async Task LogoutAsync_ReturnsFailureWhenSessionCookieIsMissing()
	{
		var memory = MemoryPrincipalServices.Create();
		var options = Options.Create(new AspNetIdentityOptions());

		var service = new AspNetIdentityService(
			memory.Authentication,
			memory.SessionService,
			new AspNetAuthSessionReader(options),
			new AspNetAuthSessionWriter(options)
		);

		var result = await service.LogoutAsync(new DefaultHttpContext());
		Assert.True(result.IsFailure);
		Assert.Equal("principal.unauthorized", result.Error.Code);
	}
}
