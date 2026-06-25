namespace Leva.Framework.Identity;

/// <summary>
/// Describes one person, system, or external principal known by the application.
/// </summary>
public sealed record Identity(
	IdentityId Id,
	string DisplayName,
	string? Email = null,
	IReadOnlySet<IdentityRole>? Roles = null,
	IReadOnlySet<IdentityPermission>? Permissions = null,
	IReadOnlyList<IdentityClaim>? Claims = null
)
{
	public IReadOnlySet<IdentityRole> Roles { get; init; } = Roles ?? new HashSet<IdentityRole>();
	public IReadOnlySet<IdentityPermission> Permissions { get; init; } =
		Permissions ?? new HashSet<IdentityPermission>();
	public IReadOnlyList<IdentityClaim> Claims { get; init; } = Claims ?? [];
}
