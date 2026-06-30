using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetGoogleContractTests
{
	[Fact]
	public void Map_CreatesGooglePrincipal()
	{
		var mapper = new AspNetGooglePrincipalMapper();
		var principal = mapper.Map(new AspNetGoogleUserInfo(
			"google-1",
			"User One",
			"user@example.com",
			true,
			"https://example.com/picture.png"
		););

		Assert.Equal(new PrincipalId("google:google-1"), principal.Id);
		Assert.Equal("User One", principal.DisplayName);
		Assert.Equal("user@example.com", principal.Email);

		Assert.Contains(
			principal.Claims,
			claim => claim.Type == AspNetGoogleDefaults.SubjectClaimType && claim.Value == "google-1"
		);
		Assert.Contains(principal.Claims, claim => claim.Type == AspNetGoogleDefaults.PictureClaimType);
	}

	[Fact]
	public void CreateAuthorizationUrl_WritesStateCookieAndUsesGoogleEndpoint()
	{
		var context = new DefaultHttpContext();
		context.Request.Scheme = "https";
		context.Request.Host = new HostString("example.com");

		var service = CreateService();
		var url = service.CreateAuthorizationUrl(context, "/after-login");

		Assert.StartsWith(AspNetGoogleDefaults.AuthorizationEndpoint, url);
		Assert.Contains("client_id=client-id", url);
		Assert.Contains("state=", url);
		Assert.Contains(".Framework.Google.State=", context.Response.Headers.SetCookie.ToString());
	}

	[Fact]
	public void CreateAuthorizationUrl_ReplacesExternalReturnUrlWithDefaultReturnUrl()
	{
		var context = new DefaultHttpContext();
		context.Request.Scheme = "https";
		context.Request.Host = new HostString("example.com");

		var service = CreateService();
		service.CreateAuthorizationUrl(context, "https://evil.example/steal");
		Assert.Contains(".Framework.Google.ReturnUrl=%2F", context.Response.Headers.SetCookie.ToString());
	}

	private static AspNetGoogleSignInService CreateService() =>
		new(
			new TestHttpClientFactory(),
			Options.Create(
				new AspNetGoogleOptions
				{
					ClientId = "client-id",
					ClientSecret = "client-secret",
					SecureCookie = false,
				}
			),
			new AspNetIdentityService(
				new AuthenticationService([], new AuthSessionService(new EmptyAuthSessionStore())),
				new AuthSessionService(new EmptyAuthSessionStore()),
				new AspNetAuthSessionReader(Options.Create(new AspNetIdentityOptions())),
				new AspNetAuthSessionWriter(Options.Create(new AspNetIdentityOptions()))
			)
		);
}
