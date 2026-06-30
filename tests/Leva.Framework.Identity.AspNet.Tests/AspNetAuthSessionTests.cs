using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetAuthSessionTests
{
	[Fact]
	public void Read_ReturnsSessionIdFromCookie()
	{
		var reader = new AspNetAuthSessionReader(Options.Create(new AspNetIdentityOptions()));
		var context = new DefaultHttpContext();
		context.Request.Headers.Cookie = ".Framework.AuthSession=session-1";

		var sessionId = reader.Read(context);
		Assert.Equal(new AuthSessionId("session-1"), sessionId);
	}

	[Fact]
	public void Read_ReturnsSessionIdFromHeaderWhenEnabled()
	{
		var reader = new AspNetAuthSessionReader(
			Options.Create(new AspNetIdentityOptions { AllowHeaderSession = true })
		);
		var context = new DefaultHttpContext();

		context.Request.Headers["Auth-Session"] = "session-1";
		var sessionId = reader.Read(context);
		Assert.Equal(new AuthSessionId("session-1"), sessionId);
	}

	[Fact]
	public void Write_AddsSessionCookie()
	{
		var writer = new AspNetAuthSessionWriter(Options.Create(new AspNetIdentityOptions { SecureCookie = false }));
		var context = new DefaultHttpContext();
		writer.Write(context, new AuthSessionId("session-1"));

		Assert.Contains(".Framework.AuthSession=session-1", context.Response.Headers.SetCookie.ToString());
	}

	[Fact]
	public void Delete_DeletesSessionCookie()
	{
		var writer = new AspNetAuthSessionWriter(Options.Create(new AspNetIdentityOptions { SecureCookie = false }));
		var context = new DefaultHttpContext();
		writer.Delete(context);

		Assert.Contains(".Framework.AuthSession=", context.Response.Headers.SetCookie.ToString());
	}
}
