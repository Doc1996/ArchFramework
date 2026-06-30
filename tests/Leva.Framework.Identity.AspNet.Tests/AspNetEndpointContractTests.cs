using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetEndpointContractTests
{
	[Fact]
	public void AspNetLoginRequest_ToAuthenticationRequest_MapsValues()
	{
		var request = new AspNetLoginRequest(
			"local.secret",
			"user",
			"secret",
			Properties: new Dictionary<string, string> { ["device"] = "kiosk" }
		);
		var authentication = request.ToAuthenticationRequest();

		Assert.Equal(new AuthenticationMethod("local.secret"), authentication.Method);
		Assert.Equal("user", authentication.Name);
		Assert.Equal("secret", authentication.Secret);
		Assert.Equal("kiosk", authentication.Properties["device"]);
	}

	[Fact]
	public void AspNetPrincipalResponse_FromSession_MapsPrincipalAndSession()
	{
		var principal = new Principal(
			new PrincipalId("principal-1"),
			"User One",
			Roles: new HashSet<PrincipalRole> { new("admin") },
			Permissions: new HashSet<PrincipalPermission> { new("plans.create") }
		);

		var session = new AuthSession(
			new AuthSessionId("session-1"),
			principal,
			AuthSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);
		var response = AspNetPrincipalResponse.FromSession(session);

		Assert.True(response.IsAuthenticated);
		Assert.Equal("session-1", response.SessionId);
		Assert.Equal("principal-1", response.PrincipalId);

		Assert.Contains("admin", response.Roles!);
		Assert.Contains("plans.create", response.Permissions!);
	}
}
