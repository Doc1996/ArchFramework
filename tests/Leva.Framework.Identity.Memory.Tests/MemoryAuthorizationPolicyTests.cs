using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

public sealed class MemoryAuthorizationPolicyTests
{
	[Fact]
	public async Task AuthorizeAsync_AllowsSignedInActiveSession()
	{
		var requirement = AuthorizationRequirement.SignedIn;
		var principal = TestPrincipal();
		var session = TestSession(principal);
		var policy = new MemoryAuthorizationPolicy();

		var result = await policy.AuthorizeAsync(new AuthorizationRequest(principal, requirement, session));
		var authorization = ResultAssert.Success(result);
		Assert.True(authorization.IsAuthorized);
	}

	[Fact]
	public async Task AuthorizeAsync_AllowsRolePermissionAndClaim()
	{
		var principal = TestPrincipal();
		var policy = new MemoryAuthorizationPolicy();
		var role = ResultAssert.Success(
			await policy.AuthorizeAsync(
				new AuthorizationRequest(principal, AuthorizationRequirement.Role(new PrincipalRole("admin")))
			)
		);

		var permission = ResultAssert.Success(
			await policy.AuthorizeAsync(
				new AuthorizationRequest(
					principal,
					AuthorizationRequirement.Permission(new PrincipalPermission("plans.edit"))
				)
			)
		);

		var claim = ResultAssert.Success(
			await policy.AuthorizeAsync(
				new AuthorizationRequest(principal, AuthorizationRequirement.Claim("email_verified", "true"))
			)
		);

		Assert.True(role.IsAuthorized);
		Assert.True(permission.IsAuthorized);
		Assert.True(claim.IsAuthorized);
	}

	[Fact]
	public async Task AuthorizeAsync_DeniesMissingPermission()
	{
		var requirement = AuthorizationRequirement.Permission(new PrincipalPermission("plans.delete"));
		var policy = new MemoryAuthorizationPolicy();
		var result = await policy.AuthorizeAsync(new AuthorizationRequest(TestPrincipal(), requirement));
		var authorization = ResultAssert.Success(result);

		Assert.False(authorization.IsAuthorized);
		Assert.Equal(requirement, authorization.Requirement);
	}

	private static Principal TestPrincipal()
	{
		return new(
			new PrincipalId("principal-1"),
			"User One",
			Roles: new HashSet<PrincipalRole> { new("admin") },
			Permissions: new HashSet<PrincipalPermission> { new("plans.edit") },
			Claims: [new PrincipalClaim("email_verified", "true")]
		);
	}

	private static PrincipalSession TestSession(Principal principal)
	{
		return new(
			new PrincipalSessionId("session-1"),
			principal,
			PrincipalSessionStatus.Active,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow
		);
	}
}
