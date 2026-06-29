namespace Leva.Framework.Identity.Local;

/// <summary>
/// Provides standard local authentication methods.
/// </summary>
public static class LocalAuthenticationMethods
{
	public static AuthenticationMethod Secret { get; } = new("local.secret");
}
