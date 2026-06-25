namespace Leva.Framework.Identity;

/// <summary>
/// Describes one signed-in, role, permission, claim, or application-specific requirement.
/// </summary>
public sealed record AuthorizationRequirement(string Type, string Value)
{
	public static AuthorizationRequirement SignedIn { get; } = new("SignedIn", "true");

	public static AuthorizationRequirement Permission(IdentityPermission permission) =>
		new("Permission", permission.Value);

	public static AuthorizationRequirement Role(IdentityRole role) => new("Role", role.Value);

	public static AuthorizationRequirement Claim(string type, string value) => new($"Claim:{type}", value);

	public override string ToString() => $"{Type}:{Value}";
}
