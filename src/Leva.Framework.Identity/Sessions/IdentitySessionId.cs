namespace Leva.Framework.Identity;

/// <summary>
/// Identifies one active or remembered identity session.
/// </summary>
public readonly record struct IdentitySessionId(string Value)
{
	public static IdentitySessionId New() => new(Guid.NewGuid().ToString("N"));

	public override string ToString() => Value;
}
