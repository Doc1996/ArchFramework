namespace Leva.Framework.Identity;

/// <summary>
/// Identifies one application identity.
/// </summary>
public readonly record struct IdentityId(string Value)
{
	public static IdentityId New() => new(Guid.NewGuid().ToString("N"));

	public override string ToString() => Value;
}
