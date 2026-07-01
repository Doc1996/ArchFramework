namespace Leva.Framework.Execution;

/// <summary>
/// Identifies one execution attempt.
/// </summary>
public readonly record struct ExecutionId(Guid Value)
{
	public static ExecutionId New() => new(Guid.NewGuid());
}
