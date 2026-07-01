using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Engine composition root. Hosts use context; states receive access.
/// </summary>
public sealed class Context(
	IClock clock,
	ILogSink logSink,
	RuntimeLog runtimeLog,
	IEventQueue eventQueue,
	StateMachine stateMachine,
	EventDispatcher eventDispatcher,
	EventLoop eventLoop,
	RoutineRunner routineRunner,
	BehaviorRunner behaviorRunner,
	AlarmBoard alarmBoard,
	StatusBoard statusBoard
)
{
	private const string AlarmsKey = "Alarms";
	private const string StatusesKey = "Statuses";

	public IClock Clock { get; } = clock;
	public ILogSink LogSink { get; } = logSink;
	public RuntimeLog RuntimeLog { get; } = runtimeLog;
	public IEventQueue EventQueue { get; } = eventQueue;
	public StateMachine StateMachine { get; } = stateMachine;
	public EventDispatcher EventDispatcher { get; } = eventDispatcher;
	public EventLoop EventLoop { get; } = eventLoop;
	public RoutineRunner RoutineRunner { get; } = routineRunner;
	public BehaviorRunner BehaviorRunner { get; } = behaviorRunner;
	public AlarmBoard AlarmBoard { get; } = alarmBoard;
	public StatusBoard StatusBoard { get; } = statusBoard;

	public Snapshot CreateSnapshot(IReadOnlyDictionary<string, object?>? data = null)
	{
		if (StateMachine.CurrentStateId is not { } stateId)
			throw new InvalidOperationException("Cannot create snapshot before state machine is started.");

		var snapshotData = new Dictionary<string, object?>(data ?? new Dictionary<string, object?>())
		{
			[AlarmsKey] = AlarmBoard.AlarmEntries.ToList(),
			[StatusesKey] = StatusBoard.StatusEntries.ToList(),
		};

		var snapshot = new Snapshot(stateId, Clock.UtcNow, snapshotData);
		RuntimeLog.Add(
			LogCategory.Snapshot,
			"Snapshot created.",
			new
			{
				StateId = stateId,
				AlarmCount = AlarmBoard.AlarmEntries.Count,
				StatusCount = StatusBoard.StatusEntries.Count,
			}
		);

		return snapshot;
	}

	public async Task LoadSnapshotAsync(Snapshot snapshot, CancellationToken token = default)
	{
		AlarmBoard.ClearAll();
		foreach (var alarmEntry in GetDataItems<AlarmEntry>(snapshot, AlarmsKey))
			AlarmBoard.Raise(alarmEntry);

		StatusBoard.ClearAll();
		foreach (var statusEntry in GetDataItems<StatusEntry>(snapshot, StatusesKey))
			StatusBoard.Set(statusEntry);

		if (StateMachine.CurrentStateId is null)
			await StateMachine.StartAsync(snapshot.StateId, token);
		else
		{
			StateMachine.RuntimeAccess.Transition.To(snapshot.StateId, "Snapshot loaded.");
			await StateMachine.ApplyTransitionsAsync(token);
		}

		RuntimeLog.Add(
			LogCategory.Snapshot,
			"Snapshot loaded.",
			new
			{
				StateId = snapshot.StateId,
				AlarmCount = AlarmBoard.AlarmEntries.Count,
				StatusCount = StatusBoard.StatusEntries.Count,
			}
		);
	}

	private static IEnumerable<TItem> GetDataItems<TItem>(Snapshot snapshot, string key)
	{
		if (!snapshot.Data.TryGetValue(key, out var value) || value is null)
			return [];

		return value switch
		{
			IEnumerable<TItem> items => items,
			TItem item => [item],
			_ => [],
		};
	}
}
