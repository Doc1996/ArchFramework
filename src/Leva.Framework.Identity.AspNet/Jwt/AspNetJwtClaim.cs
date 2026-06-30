namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Represents a framework principal claim stored in the built in JWT payload.
/// </summary>
public sealed record AspNetJwtClaim(string Type, string Value);
