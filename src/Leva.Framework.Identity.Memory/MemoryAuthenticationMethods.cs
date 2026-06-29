namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Provides standard in-memory authentication methods.
/// </summary>
public static class MemoryAuthenticationMethods
{
	public static AuthenticationMethod Principal { get; } = new("memory.principal");
}
