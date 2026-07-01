using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Executes one request and returns a typed result.
/// </summary>
public interface IExecution<TRequest, TResult>
{
	Task<Result<TResult>> ExecuteAsync(TRequest request, ExecutionProgress progress, CancellationToken token = default);
}
