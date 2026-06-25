namespace Leva.Framework.Identity;

/// <summary>
/// Identifies one active or remembered principal session.
/// </summary>
public readonly record struct PrincipalSessionId(string Value)
{
	public static PrincipalSessionId New() => new(Guid.NewGuid().ToString("N"));

	public override string ToString() => Value;
}
