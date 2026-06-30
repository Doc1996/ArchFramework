namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Represents the built in JWT header.
/// </summary>
internal sealed record AspNetJwtHeader(string Algorithm, string Type);
