namespace Leva.Framework.Core;

/// <summary>
/// Small framework error value used by result-returning operations.
/// </summary>
public readonly record struct Error(string Code, string Message)
{
	public static Error None => new(string.Empty, string.Empty);
	public bool IsNone => string.IsNullOrWhiteSpace(Code);
}
