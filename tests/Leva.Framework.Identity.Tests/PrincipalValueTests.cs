using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class PrincipalValueTests
{
	[Fact]
	public void PrincipalIdNew_CreatesNonEmptyValue()
	{
		var id = PrincipalId.New();
		Assert.False(string.IsNullOrWhiteSpace(id.Value));
		Assert.Equal(id.Value, id.ToString());
	}

	[Fact]
	public void AuthSessionIdNew_CreatesNonEmptyValue()
	{
		var id = AuthSessionId.New();
		Assert.False(string.IsNullOrWhiteSpace(id.Value));
		Assert.Equal(id.Value, id.ToString());
	}

	[Fact]
	public void Principal_DefaultCollectionsAreEmpty()
	{
		var principal = new Principal(new PrincipalId("user-1"), "User One");

		Assert.Empty(principal.Roles);
		Assert.Empty(principal.Permissions);
		Assert.Empty(principal.Claims);
	}

	[Fact]
	public void Principal_KeepsRolesPermissionsAndClaims()
	{
		var role = new PrincipalRole("admin");
		var permission = new PrincipalPermission("plans.edit");
		var claim = new PrincipalClaim("email_verified", "true");

		var principal = new Principal(
			new PrincipalId("user-1"),
			"User One",
			"user@example.com",
			new HashSet<PrincipalRole> { role },
			new HashSet<PrincipalPermission> { permission },
			[claim]
		);

		Assert.Contains(role, principal.Roles);
		Assert.Contains(permission, principal.Permissions);
		Assert.Contains(claim, principal.Claims);
	}

	[Fact]
	public void AuthorizationRequirement_ToStringShowsTypeAndValue()
	{
		var requirement = AuthorizationRequirement.Permission(new PrincipalPermission("plans.edit"));
		Assert.Equal("Permission:plans.edit", requirement.ToString());
	}
}
