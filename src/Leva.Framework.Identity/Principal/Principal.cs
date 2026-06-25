namespace Leva.Framework.Identity;

/// <summary>
/// Describes one person, system, or external principal known by the application.
/// </summary>
public sealed record Principal(
	PrincipalId Id,
	string DisplayName,
	string? Email = null,
	IReadOnlySet<PrincipalRole>? Roles = null,
	IReadOnlySet<PrincipalPermission>? Permissions = null,
	IReadOnlyList<PrincipalClaim>? Claims = null
)
{
	public IReadOnlySet<PrincipalRole> Roles { get; init; } = Roles ?? new HashSet<PrincipalRole>();
	public IReadOnlySet<PrincipalPermission> Permissions { get; init; } =
		Permissions ?? new HashSet<PrincipalPermission>();
	public IReadOnlyList<PrincipalClaim> Claims { get; init; } = Claims ?? [];
}
