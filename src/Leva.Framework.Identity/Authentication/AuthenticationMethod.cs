namespace Leva.Framework.Identity;

/// <summary>
/// Identifies one host-defined authentication method.
/// </summary>
public readonly record struct AuthenticationMethod(string Value)
{
	public override string ToString() => Value;
}
