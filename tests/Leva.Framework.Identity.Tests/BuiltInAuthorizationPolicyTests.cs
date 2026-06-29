using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class BuiltInAuthorizationPolicyTests
{
	[Fact]
	public async Task AuthorizeAsync_AllowsActiveSignedInSession()
	{
		var principal = TestPrincipal();
		var session = TestSession(principal, AuthSessionStatus.Active);
		var policy = new BuiltInAuthorizationPolicy();

		var authorization = await AuthorizeAsync(policy, principal, AuthorizationRequirement.SignedIn, session);
		Assert.True(authorization.IsAuthorized);
	}

	[Fact]
	public async Task AuthorizeAsync_DeniesSignedInRequirementWithoutActiveSession()
	{
		var principal = TestPrincipal();
		var policy = new BuiltInAuthorizationPolicy();

		var authorization = await AuthorizeAsync(policy, principal, AuthorizationRequirement.SignedIn);
		Assert.False(authorization.IsAuthorized);
	}

	[Fact]
	public async Task AuthorizeAsync_AllowsRolePermissionAndClaimRequirements()
	{
		var principal = TestPrincipal();
		var policy = new BuiltInAuthorizationPolicy();

		Assert.True(
			(
				await AuthorizeAsync(policy, principal, AuthorizationRequirement.Role(new PrincipalRole("admin")))
			).IsAuthorized
		);

		Assert.True(
			(
				await AuthorizeAsync(
					policy,
					principal,
					AuthorizationRequirement.Permission(new PrincipalPermission("plans.edit"))
				)
			).IsAuthorized
		);

		Assert.True(
			(
				await AuthorizeAsync(policy, principal, AuthorizationRequirement.Claim("department", "planning"))
			).IsAuthorized
		);
	}

	private static async Task<AuthorizationResult> AuthorizeAsync(
		BuiltInAuthorizationPolicy policy,
		Principal principal,
		AuthorizationRequirement requirement,
		AuthSession? session = null
	)
	{
		var result = await policy.AuthorizeAsync(new AuthorizationRequest(principal, requirement, session));
		return ResultAssert.Success(result);
	}

	private static Principal TestPrincipal() =>
		new(
			new PrincipalId("principal-1"),
			"User One",
			Roles: [new PrincipalRole("admin")],
			Permissions: [new PrincipalPermission("plans.edit")],
			Claims: [new PrincipalClaim("department", "planning")]
		);

	private static AuthSession TestSession(Principal principal, AuthSessionStatus status) =>
		new(new AuthSessionId("session-1"), principal, status, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
