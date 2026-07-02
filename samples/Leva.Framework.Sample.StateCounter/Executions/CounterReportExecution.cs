using Leva.Framework.Core;
using Leva.Framework.Execution;

namespace Leva.Framework.Sample.StateCounter;

internal sealed class CounterReportExecution : IExecution<CounterReportRequest, string>
{
	public Task<Result<string>> ExecuteAsync(
		CounterReportRequest request,
		ExecutionProgress progress,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		progress.Report("Creating counter report", 100);

		return Task.FromResult(Result<string>.Ok($"Counter finished with value {request.Count}."));
	}
}
