using Leva.Framework.Fakes;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetJwtTests
{
	[Fact]
	public void CreateAndValidateToken_RoundTripsAuthSession()
	{
		var service = CreateService();
		var principal = new Principal(
			new PrincipalId("principal-1"),
			"User One",
			"user@example.com",
			new HashSet<PrincipalRole> { new("admin") },
			new HashSet<PrincipalPermission> { new("plans.create") },
			new[] { new PrincipalClaim("department", "engineering") }
		);

		var session = new AuthSession(
			new AuthSessionId("session-1"),
			principal,
			AuthSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);

		var token = ResultAssert.Success(service.CreateToken(session));
		var validated = ResultAssert.Success(service.ValidateToken(token.AccessToken));

		Assert.Equal("Bearer", token.TokenType);
		Assert.Equal(session.SessionId, validated.SessionId);
		Assert.Equal(principal.Id, validated.Principal.Id);

		Assert.Contains(new PrincipalRole("admin"), validated.Principal.Roles);
		Assert.Contains(new PrincipalPermission("plans.create"), validated.Principal.Permissions);
	}

	[Fact]
	public void ValidateToken_ReturnsFailureForInvalidToken()
	{
		var service = CreateService();
		var result = service.ValidateToken("invalid-token");

		Assert.True(result.IsFailure);
		Assert.Equal("principal.unauthorized", result.Error.Code);
	}

	private static AspNetJwtTokenService CreateService() =>
		new(
			Options.Create(
				new AspNetJwtOptions
				{
					Issuer = "tests",
					Audience = "tests",
					SigningKey = "12345678901234567890123456789012",
				}
			)
		);
}
