# Framework.Engine

`Framework.Engine` is the runtime implementation of the framework. It processes queued events, updates statuses and alarms, runs states, routines, and behaviors, applies requested transitions, tracks commands, creates and loads snapshots, and records a structured runtime history. The engine owns workflow mechanics, while applications provide concrete states, access objects, services, devices, UI, storage, and domain behavior.

## Design rules

States, routines, behaviors, supervisors, and services request work; the engine applies workflow changes in one controlled runtime path. Transitions are requested through `ITransition`, queued by `QueuedTransition`, and applied by `StateMachine`.

Boards hold current or tracked runtime memory. `AlarmBoard`, `StatusBoard`, and `CommandBoard` expose typed entry collections and support set/clear-style operations. `RuntimeLog` is the only chronological log; transitions, events, alarms, statuses, commands, routines, behaviors, and snapshots are recorded there instead of having separate log classes.

Runtime classes stay concrete unless an interface is needed for a real boundary or for generic access erasure. Internal helpers such as `SyncList` and `SyncDictionary` keep repetitive locking out of boards, logs, and sinks.

## Dependencies

`Framework.Engine` depends on `Framework.Core`. It should not depend on UI, storage implementations, notifications, authentication, devices, or application projects.

```text
Application
  -> Framework.Engine
  -> Framework.Core
```

## Types

`AlarmBoard` - Stores current active alarm entries and records alarm changes in `RuntimeLog`.
`BehaviorBinding<TAccess>` - Binds one behavior to its typed access creation function.
`BehaviorRunner` - Runs fallback behaviors in binding order.
`CommandBoard` - Stores tracked command entries and records command lifecycle changes in `RuntimeLog`.
`CommandHandle` - Represents one tracked command returned to workflow or service code.
`Context` - Exposes the assembled engine runtime services and snapshot helpers.
`ContextBuilder` - Builds and configures a runtime context.
`EventDispatcher` - Routes one event through status, alarm, state, routine, behavior, and transition handling.
`EventLoop` - Dequeues and dispatches events continuously or one at a time.
`EventQueue` - Stores immediate and delayed events by priority and supports delayed event cancellation.
`IAlarmSupervisor` - Defines global alarm supervision behavior.
`IBehaviorBinding` - Provides non-generic access to behavior bindings.
`IEventQueue` - Defines the event queue boundary.
`IRoutineBinding` - Provides non-generic access to routine bindings.
`IStateBinding` - Provides non-generic access to state bindings.
`IStatusUpdater` - Maps events into status updates.
`LogCategory` - Classifies runtime log entries by broad engine area.
`LogEntry` - Represents one structured item in `RuntimeLog`.
`MemoryTraceSink` - Stores trace entries in memory.
`NullAlarmSupervisor` - Provides no-op alarm supervision.
`NullStatusUpdater` - Provides no-op status updating.
`NullTraceSink` - Ignores trace entries.
`QueuedEvent` - Wraps an event with queue metadata.
`QueuedTransition` - Stores requested transitions until the state machine applies them.
`RoutineBinding<TAccess>` - Binds one routine to its typed access creation function.
`RoutineRunner` - Runs the active routine.
`RuntimeLog` - Stores chronological runtime history and mirrors entries to the trace sink.
`StateBinding<TAccess>` - Binds one state to its typed access creation function.
`StateMachine` - Owns the current state and applies requested transitions.
`StatusBoard` - Stores latest known status entries and records status changes in `RuntimeLog`.
`SyncDictionary<TKey, TValue>` - Provides a small synchronized dictionary helper for engine internals.
`SyncList<T>` - Provides a small synchronized list helper for engine internals.
`SystemClock` - Provides system time through `IClock`.
