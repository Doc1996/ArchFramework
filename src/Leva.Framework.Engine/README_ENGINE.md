# Leva.Framework.Engine

`Leva.Framework.Engine` is the runtime implementation of ArchFramework. It executes the event-driven state-machine model defined by Core.

## Purpose and dependencies

Engine turns Core contracts into deterministic runtime behavior. It owns event processing, transition draining, state execution, routine execution, fallback behavior execution, alarms, statuses, commands, runtime history, tracing, and snapshot coordination. `Leva.Framework.Engine` depends on `Leva.Framework.Core`. Engine must not depend on Fakes, Storage implementations, UI providers, notification providers, authentication providers, database providers, devices, or application projects.

```text
Leva.Framework.Engine
  -> Leva.Framework.Core

Applications / hosts
  -> Leva.Framework.Engine
  -> Leva.Framework.Core

Leva.Framework.Fakes
future provider libraries
  -> Leva.Framework.Engine
  -> Leva.Framework.Core
```

## Project overview

Engine is the layer that makes application behavior run through one controlled path. Events enter an event queue, the event loop dispatches them, global runtime components observe them, the current state handles them, an active routine may continue, fallback behaviors may run, and requested transitions are applied only after event handling finishes.

```text
EventQueue
-> EventLoop
-> EventDispatcher
-> StatusUpdater
-> AlarmSupervisor
-> StateMachine
-> RoutineRunner
-> BehaviorRunner
-> transition drain
```

This separation keeps state changes predictable. Runtime code requests transitions through `ITransition`, but `StateMachine` decides when they are applied. That avoids arbitrary code changing the active state in the middle of event processing.

Engine keeps latest-known runtime facts in boards and chronological history in the runtime log.

```text
AlarmBoard   -> active alarms
StatusBoard  -> latest statuses
CommandBoard -> tracked commands
RuntimeLog   -> chronological runtime history
TraceSink    -> diagnostic output
```

The engine should remain application-independent. Applications provide concrete states, events, access objects, services, screens, persistence, notifications, and domain rules. Engine only provides reusable runtime mechanics.

## Files and classes

### Composition and runtime execution

`Context` - Main runtime composition object that owns engine services and exposes controlled runtime capabilities.
`EventLoop` - Runs queued events through the dispatcher and coordinates continuous runtime processing.
`EventDispatcher` - Sends each event through global runtime observers, current state, active routine, fallback behaviors, and transition draining.
`EventQueue` - Priority-aware queue for events waiting to be processed by the engine.

### State machine and transitions

`StateMachine<TAccess>` - Owns the current state, calls enter/exit lifecycle methods, and applies state changes.
`TransitionController` - Captures transition requests so they can be drained and applied safely after event handling.
`TransitionRequest` - Represents one requested state change.

### Routines and behaviors

`RoutineRunner<TAccess>` - Runs an active routine and tracks routine lifecycle behavior.
`RoutineBinding<TAccess>` - Connects routine identifiers or triggers to routine instances/factories.
`BehaviorRunner<TAccess>` - Runs fallback behaviors when an event is not handled by state or routine logic.
`BehaviorBinding<TAccess>` - Defines behavior order and binding rules for fallback handling.

### Runtime boards

`AlarmBoard` - Stores active alarms and exposes safe snapshots of alarm entries.
`AlarmSupervisor` - Updates alarm state from runtime events and application actions.
`StatusBoard` - Stores latest-known status values and exposes safe snapshots.
`StatusUpdater` - Updates status entries from runtime events and application actions.
`CommandBoard` - Tracks command lifecycle entries and exposes safe snapshots.

### Runtime logging and tracing

`RuntimeLog` - Stores chronological runtime entries for diagnostics, observation, and later persistence.
`RuntimeEntry` - Represents one recorded runtime action, decision, event, transition, status update, alarm change, command change, or trace item.
`RuntimeEntryKind` - Categorizes runtime log entries.
`MemoryTraceSink` - Thread-safe in-memory trace sink for diagnostics and tests.
`CompositeTraceSink` - Forwards trace entries to multiple trace sinks.
`NullTraceSink` - Trace sink implementation that intentionally ignores trace entries.

### Snapshot support

`SnapshotProvider` - Produces runtime snapshots from the current engine state when available.
`SnapshotRestorer` - Restores supported runtime state from a saved `Snapshot`.

### Synchronization helpers

`SyncList<T>` - Small synchronized list helper used by engine internals.
`SyncDictionary<TKey, TValue>` - Small synchronized dictionary helper used by engine internals.
