namespace Leva.Framework.Identity;

/// <summary>
/// Represents one permission that can be required by application logic.
/// </summary>
public readonly record struct PrincipalPermission(string Value)
{
	public override string ToString() => Value;
}
