namespace Leva.Framework.Identity;

/// <summary>
/// Carries the identity, session, requirement, and optional contextual properties for authorization.
/// </summary>
public sealed record AuthorizationRequest(
	Identity? Identity,
	AuthorizationRequirement Requirement,
	IdentitySession? Session = null,
	IReadOnlyDictionary<string, object?>? Properties = null
)
{
	public IReadOnlyDictionary<string, object?> Properties { get; init; } =
		Properties ?? new Dictionary<string, object?>();
}
