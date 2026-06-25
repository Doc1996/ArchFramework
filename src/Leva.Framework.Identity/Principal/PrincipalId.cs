namespace Leva.Framework.Identity;

/// <summary>
/// Identifies one application principal.
/// </summary>
public readonly record struct PrincipalId(string Value)
{
	public static PrincipalId New() => new(Guid.NewGuid().ToString("N"));

	public override string ToString() => Value;
}
