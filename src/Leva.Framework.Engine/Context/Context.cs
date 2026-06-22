using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Engine composition root. Hosts use context; states receive access.
/// </summary>
public sealed class Context(
	IClock clock,
	ITraceSink traceSink,
	RuntimeLog runtimeLog,
	IEventQueue eventQueue,
	StateMachine stateMachine,
	EventDispatcher eventDispatcher,
	EventLoop eventLoop,
	RoutineRunner routineRunner,
	BehaviorRunner behaviorRunner,
	AlarmBoard alarmBoard,
	StatusBoard statusBoard,
	CommandBoard commandBoard
)
{
	private const string AlarmsKey = "Alarms";
	private const string StatusesKey = "Statuses";
	private const string CommandsKey = "Commands";

	public IClock Clock { get; } = clock;
	public ITraceSink TraceSink { get; } = traceSink;
	public RuntimeLog RuntimeLog { get; } = runtimeLog;
	public IEventQueue EventQueue { get; } = eventQueue;
	public StateMachine StateMachine { get; } = stateMachine;
	public EventDispatcher EventDispatcher { get; } = eventDispatcher;
	public EventLoop EventLoop { get; } = eventLoop;
	public RoutineRunner RoutineRunner { get; } = routineRunner;
	public BehaviorRunner BehaviorRunner { get; } = behaviorRunner;
	public AlarmBoard AlarmBoard { get; } = alarmBoard;
	public StatusBoard StatusBoard { get; } = statusBoard;
	public CommandBoard CommandBoard { get; } = commandBoard;

	public Snapshot CreateSnapshot(IReadOnlyDictionary<string, object?>? data = null)
	{
		if (StateMachine.CurrentStateId is not { } stateId)
			throw new InvalidOperationException("Cannot create snapshot before state machine is started.");

		var snapshotData = new Dictionary<string, object?>(data ?? new Dictionary<string, object?>())
		{
			[AlarmsKey] = AlarmBoard.AlarmEntries.ToList(),
			[StatusesKey] = StatusBoard.StatusEntries.ToList(),
			[CommandsKey] = CommandBoard.CommandEntries.ToList(),
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
				CommandCount = CommandBoard.CommandEntries.Count,
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

		CommandBoard.ClearAll();
		foreach (var commandEntry in GetDataItems<CommandEntry>(snapshot, CommandsKey))
			CommandBoard.Set(commandEntry);

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
				CommandCount = CommandBoard.CommandEntries.Count,
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
