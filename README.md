# ArchFramework

ArchFramework is a clean .NET framework for event-driven, state-machine applications with pluggable infrastructure. It gives applications a reusable architecture for typed events, explicit states, routines, behaviors, transitions, runtime memory, diagnostics, and recovery snapshots.

## Architecture

The framework separates application behavior from infrastructure. Applications define their own domain events, states, routines, screens, services, storage, notifications, and policies. ArchFramework provides the runtime structure that makes this behavior deterministic, observable, and testable.

The most important idea is that application logic should not be hidden inside random services, UI callbacks, or background tasks. Important behavior should flow through events, states, routines, behaviors, and controlled transitions.

## Current libraries

```text
Leva.Framework.Core   -> shared contracts and vocabulary
Leva.Framework.Engine -> runtime implementation
Leva.Framework.Fakes  -> reusable test doubles
```

Core defines the concepts. Engine executes the concepts. Fakes make the concepts easier to test. Future libraries should stay separate so Core and Engine do not become infrastructure-heavy.

```text
Leva.Framework.Storage           future
Leva.Framework.Storage.InMemory  future
Leva.Framework.Storage.Sqlite    future
Leva.Framework.Notifications     future
Leva.Framework.Authentication    future
Leva.Framework.Presentation      future
Leva.Framework.Presentation.Blazor future
```

## Dependency direction

Framework dependencies move inward toward Core. Applications and provider libraries can depend on framework libraries, but framework libraries should not depend on applications or concrete providers.

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

Core must stay independent. Engine must stay independent from UI, storage providers, notification providers, authentication providers, devices, and application projects. Provider libraries adapt external systems; they should not be pushed into Core or Engine.

## Runtime model

The engine processes work through one controlled path.

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

An event enters the queue, is dispatched through global runtime components, reaches the active state, may continue an active routine, may fall through to fallback behaviors, and only then applies requested transitions. This keeps event handling predictable and avoids state changes happening randomly in the middle of execution.

## Access model

States, routines, and behaviors should receive narrow typed access objects instead of the full runtime `Context`. The context owns engine services and runtime wiring, but application logic should only see the capabilities it is allowed to use.

This makes states easier to understand and test. A state that only needs navigation and transitions should not receive storage, notifications, device control, or unrelated application services.

## Runtime memory

Engine memory is grouped by responsibility.

```text
AlarmBoard   -> active alarms and faults
StatusBoard  -> latest-known statuses
CommandBoard -> tracked command lifecycle entries
RuntimeLog   -> chronological runtime history
TraceSink    -> diagnostic trace output
Snapshot     -> durable recovery state
```

Boards answer “what is true now?” Runtime logs answer “what happened over time?” Trace sinks answer “what should diagnostics receive?” Snapshots answer “what state can be saved and restored?”

## Project guides

The repository contains one general README and one focused guide per implemented library.

```text
README.md    -> architecture and repository overview
README_CORE.md   -> Core purpose, dependencies, overview and files/classes
README_ENGINE.md -> Engine purpose, dependencies, overview and files/classes
README_FAKES.md  -> Fakes purpose, dependencies, overview and files/classes
```

## Build and test

From the repository root:

```bash
dotnet restore ArchFramework.slnx
dotnet build ArchFramework.slnx
dotnet test ArchFramework.slnx
```

To test one library:

```bash
dotnet test tests/Leva.Framework.Core.Tests/Leva.Framework.Core.Tests.csproj
dotnet test tests/Leva.Framework.Engine.Tests/Leva.Framework.Engine.Tests.csproj
dotnet test tests/Leva.Framework.Fakes.Tests/Leva.Framework.Fakes.Tests.csproj
```
