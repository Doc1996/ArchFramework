using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Execution;

namespace Leva.Framework.Sample.StateCounter;

internal sealed class CounterAccess(
	ITransition transition,
	ILogSink logSink,
	CounterModel model,
	ExecutionRunner executions,
	Func<AlarmBoard> alarmBoard,
	Func<StatusBoard> statusBoard
) : IAccess
{
	// IAccess is the small runtime surface every state receives from the Engine.
	// The sample extends it with only the application model and services needed by this workflow.
	public ITransition Transition { get; } = transition;
	public ILogSink LogSink { get; } = logSink;
	public CounterModel Model { get; } = model;
	public ExecutionRunner Executions { get; } = executions;

	// These delegates point to the Context-owned boards. They avoid passing the whole Context into states.
	public AlarmBoard AlarmBoard => alarmBoard();
	public StatusBoard StatusBoard => statusBoard();
}
