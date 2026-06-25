namespace Leva.Framework.Identity;

/// <summary>
/// Carries method-specific data needed by an authentication policy.
/// </summary>
public sealed record AuthenticationRequest(
	AuthenticationMethod Method,
	string? Name = null,
	string? Secret = null,
	string? Token = null,
	IReadOnlyDictionary<string, string>? Properties = null
)
{
	public IReadOnlyDictionary<string, string> Properties { get; init; } =
		Properties ?? new Dictionary<string, string>();
}
