namespace Leva.Framework.Identity.Local;

/// <summary>
/// Stores a protected local secret and the parameters needed to verify it later.
/// </summary>
public sealed record LocalCredentialSecret(string Algorithm, int Iterations, string Salt, string Value);
