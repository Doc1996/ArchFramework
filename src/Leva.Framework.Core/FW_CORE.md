# Framework.Core

`Framework.Core` defines the shared vocabulary used by the framework, engine, applications, tests, and infrastructure libraries. It contains contracts, IDs, entries, result types, and small data records, but it does not execute workflows or own runtime behavior. The library should stay small, stable, and independent from UI, storage, devices, notifications, and application-specific code.

## Design rules

Core contains concepts, not implementations. It should define only stable contracts and simple data types that other libraries can safely depend on.

Use strongly typed IDs when a value has framework meaning. Runtime instance IDs such as `AlarmId`, `CommandId`, `RequestId`, and `EventId` can create new values with `New()`. Named concept IDs such as `StateId` and `RoutineId` are normally supplied by the application.

Use entries for stored facts or snapshots. Constructor arguments for entry-like records are ordered as consistently as practical: identity/source first, name/message next, status/category/level/value next, time fields next, optional state/error/properties last.

`Snapshot` stores framework and application recovery data as a durable snapshot. Framework runtime collections such as alarms, statuses, and commands may be stored in its `Data` dictionary by the engine.

## Dependencies

`Framework.Core` depends only on .NET base libraries. Other framework libraries may depend on Core, but Core must not depend on Engine, Presentation, Storage, Diagnostics, Services, or application projects.

```text
Application / Engine / Storage / Diagnostics / Testing
  -> Framework.Core
```

## Types

`AlarmEntry` - Represents one active alarm or fault with identity, message, level, blocking flag, raised time, and optional properties.
`AlarmId` - Identifies one alarm instance.
`AlarmLevel` - Defines alarm severity.
`Snapshot` - Represents one saved runtime point with state, creation time, and durable data for recovery.
`CommandEntry` - Represents one tracked command or external operation with identity, name, status, times, error, and optional properties.
`CommandId` - Identifies one command for correlation, timeout, completion, and recovery.
`CommandStatus` - Describes the lifecycle state of a tracked command.
`Error` - Represents a small structured failure value.
`EventId` - Identifies one event instance for logging, scheduling, cancellation, and correlation.
`EventPriority` - Defines event queue priority.
`IAccess` - Defines the minimal capability surface visible to states, routines, and behaviors.
`IBehavior<TAccess>` - Defines reusable fallback behavior for events not handled by the active state or routine.
`IClock` - Provides current runtime time through an abstraction.
`IEvent` - Represents an event payload processed by the runtime.
`IRoutine<TAccess>` - Defines a temporary multi-step workflow.
`IState<TAccess>` - Defines one application state with typed access and entry, exit, and event handling.
`ITraceSink` - Receives diagnostic trace entries.
`ITransition` - Allows transition requests without direct state-machine control.
`RequestId` - Identifies a request that expects a correlated response or completion event.
`Result` - Represents success or structured failure without a returned value.
`Result<T>` - Represents either a successful value or a structured failure.
`RoutineId` - Identifies one routine type or routine instance.
`RoutineStatus` - Describes routine lifecycle state.
`StateId` - Identifies one application state.
`StatusEntry` - Represents one latest-known status value with source, name, value, updated time, and optional properties.
`TraceEntry` - Represents one diagnostic trace item with source, message, level, created time, and optional properties.
`TraceLevel` - Defines diagnostic severity.
