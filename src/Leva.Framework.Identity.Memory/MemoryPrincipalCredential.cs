namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Connects a test or demo login name and secret to a principal.
/// </summary>
public sealed record MemoryPrincipalCredential(string Name, string Secret, PrincipalId PrincipalId);
