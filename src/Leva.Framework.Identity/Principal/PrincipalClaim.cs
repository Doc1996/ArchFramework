namespace Leva.Framework.Identity;

/// <summary>
/// Represents one external or application-defined claim attached to a principal.
/// </summary>
public sealed record PrincipalClaim(string Type, string Value);
