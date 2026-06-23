# ArchFramework

ArchFramework is a clean .NET framework for event-driven, state-machine applications. It provides reusable runtime structure for typed events, explicit states, routines, behaviors, controlled transitions, runtime memory, diagnostics, and recovery snapshots.

## Architecture summary

ArchFramework keeps important application behavior out of random services, UI callbacks, and uncontrolled background code. Work enters the runtime as typed events, flows through a deterministic dispatcher, reaches the active state or routine, can fall back to behaviors, and only then applies requested transitions. This makes the application easier to reason about, test, log, and recover.

The framework is split into small libraries. `Leva.Framework.Core` defines the shared vocabulary. `Leva.Framework.Engine` implements the runtime. `Leva.Framework.Fakes` provides reusable test doubles. Future infrastructure libraries should stay separate, for example storage, notifications, authentication, and presentation adapters.

## Library structure

```text
Leva.Framework.Core   -> shared contracts, IDs, entries, results, snapshots
Leva.Framework.Engine -> event queue, dispatcher, state machine, boards, logs
Leva.Framework.Fakes  -> fake clocks, events, queues, states, routines, logs
```

Future libraries should plug into the same architecture without forcing infrastructure into Core or Engine.

```text
Leva.Framework.Storage            future
Leva.Framework.Storage.Memory     future
Leva.Framework.Storage.Sqlite     future
Leva.Framework.Notifications      future
Leva.Framework.Authentication     future
Leva.Framework.Presentation       future
Leva.Framework.Presentation.Blazor future
```

## Dependency direction

Dependencies move inward toward Core. Core depends only on .NET. Engine depends on Core. Fakes depends on Engine and Core because it is a testing support library. Applications and future provider libraries may depend on selected framework libraries, but Core and Engine should not depend on applications, UI providers, storage providers, notification providers, authentication providers, devices, databases, or web frameworks.

```text
Leva.Framework.Core
  -> .NET only

Leva.Framework.Engine
  -> Leva.Framework.Core

Leva.Framework.Fakes
  -> Leva.Framework.Engine
  -> Leva.Framework.Core

Applications / future providers
  -> selected framework libraries
```

## Runtime flow

The engine processes events through one controlled path.

```text
EventQueue
-> EventLoop
-> EventDispatcher
-> IStatusUpdater
-> IAlarmSupervisor
-> StateMachine
-> RoutineRunner
-> BehaviorRunner
-> queued transition drain
```

An event is queued, dequeued by the loop, dispatched through global runtime hooks, handled by the active state or routine when possible, optionally handled by fallback behaviors, and then transition requests are drained. This avoids state changes happening unpredictably in the middle of event handling.

## Access model

States, routines, and behaviors should receive narrow typed access objects instead of the full runtime `Context`. The context owns runtime services and wiring. Application logic should only see the capabilities it is allowed to use, such as transition, logging, views, data, policies, navigation, or notifications depending on the application.

This keeps states smaller and easier to test. A state that only needs navigation and transitions should not receive storage, notifications, device control, or unrelated services.

## Memory model

The engine separates latest-known runtime facts from chronological history and diagnostic output.

```text
AlarmBoard   -> currently active alarms and faults
StatusBoard  -> latest-known status values
CommandBoard -> tracked command lifecycle entries
RuntimeLog   -> chronological runtime history as LogEntry values
LogSink    -> diagnostic log output
Snapshot     -> durable recovery state
```

Boards answer what is true now. The runtime log answers what happened over time. Log sinks receive diagnostic output. Snapshots capture recovery data that can later be stored and restored by the host or a future storage library.

## Project guides

```text
README.md         -> architecture and repository overview
README_CORE.md    -> Core purpose, dependencies, overview, files and classes
README_ENGINE.md  -> Engine purpose, dependencies, overview, files and classes
README_FAKES.md   -> Fakes purpose, dependencies, overview, files and classes
README_*_TESTS.md -> focused test coverage summaries
```

## Build and test

From the repository root:

```bash
dotnet restore ArchFramework.slnx
dotnet build ArchFramework.slnx
dotnet test ArchFramework.slnx
```
