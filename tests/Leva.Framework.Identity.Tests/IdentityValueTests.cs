using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class IdentityValueTests
{
	[Fact]
	public void IdentityIdNew_CreatesNonEmptyValue()
	{
		var id = IdentityId.New();
		Assert.False(string.IsNullOrWhiteSpace(id.Value));
		Assert.Equal(id.Value, id.ToString());
	}

	[Fact]
	public void IdentitySessionIdNew_CreatesNonEmptyValue()
	{
		var id = IdentitySessionId.New();
		Assert.False(string.IsNullOrWhiteSpace(id.Value));
		Assert.Equal(id.Value, id.ToString());
	}

	[Fact]
	public void Identity_DefaultCollectionsAreEmpty()
	{
		var identity = new Identity(new IdentityId("user-1"), "User One");

		Assert.Empty(identity.Roles);
		Assert.Empty(identity.Permissions);
		Assert.Empty(identity.Claims);
	}

	[Fact]
	public void Identity_KeepsRolesPermissionsAndClaims()
	{
		var role = new IdentityRole("admin");
		var permission = new IdentityPermission("plans.edit");
		var claim = new IdentityClaim("email_verified", "true");

		var identity = new Identity(
			new IdentityId("user-1"),
			"User One",
			"user@example.com",
			new HashSet<IdentityRole> { role },
			new HashSet<IdentityPermission> { permission },
			[claim]
		);

		Assert.Contains(role, identity.Roles);
		Assert.Contains(permission, identity.Permissions);
		Assert.Contains(claim, identity.Claims);
	}

	[Fact]
	public void AuthorizationRequirement_ToStringShowsTypeAndValue()
	{
		var requirement = AuthorizationRequirement.Permission(new IdentityPermission("plans.edit"));
		Assert.Equal("Permission:plans.edit", requirement.ToString());
	}
}
