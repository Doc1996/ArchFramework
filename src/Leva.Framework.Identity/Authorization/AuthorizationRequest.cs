namespace Leva.Framework.Identity;

/// <summary>
/// Carries the principal, session, requirement, and optional contextual properties for authorization.
/// </summary>
public sealed record AuthorizationRequest(
	Principal? Principal,
	AuthorizationRequirement Requirement,
	PrincipalSession? Session = null,
	IReadOnlyDictionary<string, object?>? Properties = null
)
{
	public IReadOnlyDictionary<string, object?> Properties { get; init; } =
		Properties ?? new Dictionary<string, object?>();
}
