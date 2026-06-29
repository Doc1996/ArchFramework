namespace Leva.Framework.Testing;

/// <summary>
/// Exception thrown when a framework result assertion fails.
/// </summary>
public sealed class ResultAssertionException(string message) : Exception(message);
