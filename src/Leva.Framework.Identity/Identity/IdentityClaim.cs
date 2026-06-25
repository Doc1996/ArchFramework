namespace Leva.Framework.Identity;

/// <summary>
/// Represents one external or application-defined claim attached to an identity.
/// </summary>
public sealed record IdentityClaim(string Type, string Value);
