namespace Leva.Framework.Identity;

/// <summary>
/// Represents one role assigned to a principal.
/// </summary>
public readonly record struct PrincipalRole(string Value)
{
	public override string ToString() => Value;
}
