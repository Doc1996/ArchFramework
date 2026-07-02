namespace Leva.Framework.Sample.StateCounter;

internal sealed record StateCounterResult(
	bool Passed,
	string State,
	int Count,
	string Report,
	int RuntimeEntryCount,
	int ExecutionEntryCount,
	int AlarmCount,
	int StatusCount,
	IReadOnlyList<SampleCheck> Checks,
	IReadOnlyList<LogItem> RuntimeLog,
	IReadOnlyList<ExecutionItem> Executions,
	IReadOnlyList<AlarmItem> Alarms,
	IReadOnlyList<StatusItem> Statuses
);
