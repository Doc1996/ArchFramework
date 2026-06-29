namespace Leva.Framework.Identity;

/// <summary>
/// Identifies one active or remembered auth session.
/// </summary>
public readonly record struct AuthSessionId(string Value)
{
	public static AuthSessionId New() => new(Guid.NewGuid().ToString("N"));

	public override string ToString() => Value;
}
