using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Provides shared execution error codes for cancelled, failed, and unregistered work.
/// </summary>
public static class ExecutionErrors
{
	public static Error Cancelled(ExecutionId executionId) =>
		new("execution.cancelled", $"Execution '{executionId.Value}' was cancelled.");

	public static Error Failed(ExecutionId executionId, Exception exception) =>
		new("execution.failed", $"Execution '{executionId.Value}' failed: {exception.Message}");

	public static Error NotRegistered(Type requestType, Type resultType) =>
		new(
			"execution.not_registered",
			$"No execution is registered for request '{requestType.Name}' and result '{resultType.Name}'."
		);
}
