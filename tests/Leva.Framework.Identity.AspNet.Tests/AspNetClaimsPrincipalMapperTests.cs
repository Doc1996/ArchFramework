using System.Security.Claims;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetClaimsPrincipalMapperTests
{
	[Fact]
	public void Map_CreatesClaimsPrincipalFromAuthSession()
	{
		var mapper = new AspNetClaimsPrincipalMapper(Options.Create(new AspNetIdentityOptions()));
		var principal = new Principal(
			new PrincipalId("principal-1"),
			"User One",
			"user@example.com",
			[new PrincipalRole("admin")],
			[new PrincipalPermission("plans.create")],
			[new PrincipalClaim("department", "engineering")]
		);

		var session = new AuthSession(
			new AuthSessionId("session-1"),
			principal,
			AuthSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);
		var claimsPrincipal = mapper.Map(session);

		Assert.True(claimsPrincipal.Identity?.IsAuthenticated);
		Assert.Equal("principal-1", claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
		Assert.Equal("User One", claimsPrincipal.FindFirstValue(ClaimTypes.Name));
		Assert.Equal("user@example.com", claimsPrincipal.FindFirstValue(ClaimTypes.Email));

		Assert.Equal("session-1", claimsPrincipal.FindFirstValue(AspNetClaimsPrincipalMapper.SessionIdClaimType));
		Assert.Equal("admin", claimsPrincipal.FindFirstValue(ClaimTypes.Role));
		Assert.Equal("plans.create", claimsPrincipal.FindFirstValue(AspNetClaimsPrincipalMapper.PermissionClaimType));
		Assert.Equal("engineering", claimsPrincipal.FindFirstValue("department"));
	}
}
