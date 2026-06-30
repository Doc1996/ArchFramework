using Leva.Framework.Identity.Memory;
using Leva.Framework.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetAuthSessionSourceTests
{
	[Fact]
	public async Task GetSessionAsync_LoadsSessionFromHttpContextCookie()
	{
		var store = new MemoryAuthSessionStore();
		var service = new AuthSessionService(store);
		var principal = new Principal(new PrincipalId("principal-1"), "User One");

		var session = ResultAssert.Success(await service.CreateAsync(principal));
		var context = new DefaultHttpContext();
		context.Request.Headers.Cookie = $".Framework.AuthSession={session.SessionId.Value}";

		var accessor = new HttpContextAccessor { HttpContext = context };
		var source = new AspNetAuthSessionSource(
			accessor,
			new AspNetAuthSessionReader(Options.Create(new AspNetIdentityOptions())),
			service
		);

		var result = await source.GetSessionAsync();
		var loaded = ResultAssert.Success(result);
		Assert.Equal(session, loaded);
	}

	[Fact]
	public async Task GetSessionAsync_ReturnsNullWithoutHttpContext()
	{
		var source = new AspNetAuthSessionSource(
			new HttpContextAccessor(),
			new AspNetAuthSessionReader(Options.Create(new AspNetIdentityOptions())),
			new AuthSessionService(new MemoryAuthSessionStore())
		);

		var result = await source.GetSessionAsync();
		Assert.Null(ResultAssert.Success(result));
	}
}
