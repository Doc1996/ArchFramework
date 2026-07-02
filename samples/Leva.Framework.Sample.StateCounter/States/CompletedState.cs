using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed class CompletedState : IState<CounterAccess>
{
	public StateId Id => CounterStateIds.Completed;
	public string Name => nameof(CompletedState);

	public async Task EnterAsync(CounterAccess access, CancellationToken token)
	{
		// ExecutionRunner runs typed work outside the state handler and records progress/status.
		var handle = access.Executions.Start<CounterReportRequest, string>(
			new CounterReportRequest(access.Model.Count),
			new CounterReportExecution(),
			token
		);

		var result = await handle.Completion;
		if (result.IsSuccess)
			access.Model.SetReport(result.Value!);
	}

	public Task ExitAsync(CounterAccess access, CancellationToken token) => Task.CompletedTask;
	public Task<bool> HandleAsync(CounterAccess access, IEvent appEvent, CancellationToken token) => Task.FromResult(false);
}
