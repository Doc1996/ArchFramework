namespace Leva.Framework.Core;

/// <summary>
/// Identifies one alarm instance raised by the runtime or application.
/// </summary>
public readonly record struct AlarmId(Guid Value)
{
	public static AlarmId New() => new(Guid.NewGuid());
}

/// <summary>
/// Identifies a command issued by a state, routine, behavior or application service.
/// </summary>
public readonly record struct CommandId(Guid Value)
{
	public static CommandId New() => new(Guid.NewGuid());
}

/// <summary>
/// Identifies a request that expects a correlated response or completion event.
/// </summary>
public readonly record struct RequestId(Guid Value)
{
	public static RequestId New() => new(Guid.NewGuid());
}

/// <summary>
/// Identifies one event instance so it can be logged, correlated, scheduled or cancelled.
/// </summary>
public readonly record struct EventId(Guid Value)
{
	public static EventId New() => new(Guid.NewGuid());
}

/// <summary>
/// Identifies a routine type or routine instance in runtime logs and bindings.
/// </summary>
public readonly record struct RoutineId(string Value);

/// <summary>
/// Identifies an application state in transitions, snapshots, and runtime logs.
/// </summary>
public readonly record struct StateId(string Value);
