namespace Leva.Framework.Identity;

/// <summary>
/// Represents one role assigned to an identity.
/// </summary>
public readonly record struct IdentityRole(string Value)
{
	public override string ToString() => Value;
}
