# ArchFramework

ArchFramework is a clean .NET framework for event-driven, state-machine applications. It provides reusable runtime structure for typed events, explicit states, routines, behaviors, controlled transitions, runtime memory, diagnostics, principal identity, provider-neutral storage, and recovery snapshots.

## Architecture summary

ArchFramework keeps important application behavior out of random services, UI callbacks, and uncontrolled background code. Work enters the runtime as typed events, flows through a deterministic dispatcher, reaches the active state or routine, can fall back to behaviors, and only then applies requested transitions. This makes the application easier to reason about, test, log, persist, secure, and recover.

The framework is split into small libraries. `Leva.Framework.Core` defines the shared vocabulary. `Leva.Framework.Engine` implements the runtime. `Leva.Framework.Fakes` provides reusable test doubles. `Leva.Framework.Storage` defines provider-neutral persistence contracts. Storage provider libraries implement those contracts for memory, files, and SQLite. `Leva.Framework.Identity` defines provider-neutral principal, auth session, authentication, authorization, audit, and state-facing principal access concepts. `Leva.Framework.Identity.Memory` provides an in-memory identity provider for tests and demos. `Leva.Framework.Identity.Local` provides local/offline secret-based identity for desktop, kiosk, and internal tools. `Leva.Framework.Identity.AspNet` adapts framework identity auth sessions to ASP.NET Core authentication, authorization, cookies, built in endpoints, Google sign-in, and JWT bearer tokens. `Leva.Framework.Testing` contains shared test-only assertion helpers.

## Library structure

```text
Leva.Framework.Core            -> shared contracts, IDs, entries, results, snapshots
Leva.Framework.Engine          -> event queue, dispatcher, state machine, boards, logs
Leva.Framework.Fakes           -> fake clocks, events, queues, states, routines, principal, logs
Leva.Framework.Storage         -> repository, journal, entry, version contracts
Leva.Framework.Storage.Memory  -> in-process storage provider
Leva.Framework.Storage.Files   -> local JSON/file-system storage provider
Leva.Framework.Storage.Sqlite  -> local SQLite storage provider
Leva.Framework.Identity        -> principals, auth sessions, authentication, authorization, audit
Leva.Framework.Identity.Memory -> in-memory principal/auth-session/auth provider
Leva.Framework.Identity.Local  -> local/offline credential and secret auth provider
Leva.Framework.Identity.AspNet -> ASP.NET Core auth-session, claims, endpoints, Google, JWT
Leva.Framework.Testing         -> shared test assertions and test-only helpers
```

Future libraries should plug into the same architecture without forcing infrastructure into Core or Engine.

```text
Leva.Framework.Notifications        future
Leva.Framework.Presentation.Blazor  future
```

## Dependency direction

Dependencies move inward toward Core. Core depends only on .NET. Engine depends on Core. Storage depends on Core. Identity depends on Core. Provider libraries depend on their contract libraries and Core. Fakes depends on Core, Engine, and Identity because it is a testing support library. Testing depends on Core and stays independent of a specific test runner. Applications and future provider libraries may depend on selected framework libraries, but Core and Engine should not depend on applications, UI providers, storage providers, notification providers, identity providers, devices, databases, or web frameworks.

```text
Leva.Framework.Core
  -> .NET only

Leva.Framework.Engine
  -> Leva.Framework.Core

Leva.Framework.Storage
Leva.Framework.Identity
  -> Leva.Framework.Core

Leva.Framework.Storage.Memory
Leva.Framework.Storage.Files
Leva.Framework.Storage.Sqlite
  -> Leva.Framework.Storage
  -> Leva.Framework.Core

Leva.Framework.Identity.Memory
Leva.Framework.Identity.Local
  -> Leva.Framework.Identity
  -> Leva.Framework.Core

Leva.Framework.Identity.AspNet
  -> Leva.Framework.Identity
  -> Leva.Framework.Core
  -> Microsoft.AspNetCore.App

Leva.Framework.Fakes
  -> Leva.Framework.Engine
  -> Leva.Framework.Identity
  -> Leva.Framework.Core

Leva.Framework.Testing
  -> Leva.Framework.Core

Applications / providers
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

States, routines, and behaviors should receive narrow typed access objects instead of the full runtime `Context`. The context owns runtime services and wiring. Application logic should only see the capabilities it is allowed to use, such as transition, logging, principal, views, data, policies, navigation, or notifications depending on the application.

This keeps states smaller and easier to test. A state that only needs navigation and transitions should not receive storage, notifications, device control, or unrelated services.

## Memory model

The engine separates latest-known runtime facts from chronological history and diagnostic output.

```text
AlarmBoard   -> currently active alarms and faults
StatusBoard  -> latest-known status values
CommandBoard -> tracked command lifecycle entries
RuntimeLog   -> chronological runtime history as LogEntry values
LogSink      -> diagnostic log output
Snapshot     -> durable recovery state
```

Boards answer what is true now. The runtime log answers what happened over time. Log sinks receive diagnostic output. Snapshots capture recovery data that can later be stored and restored by the host or a storage library. Identity audit sinks separately record security-relevant identity actions.

## Project guides

```text
README.md                   -> architecture and repository overview
README_CORE.md              -> Core purpose, dependencies, overview, files and classes
README_ENGINE.md            -> Engine purpose, dependencies, overview, files and classes
README_FAKES.md             -> Fakes purpose, dependencies, overview, files and classes
README_STORAGE*.md          -> Storage contracts and provider library guides
README_IDENTITY.md          -> Identity purpose, dependencies, overview, files and classes
README_IDENTITY_MEMORY.md   -> Identity.Memory provider guide
README_IDENTITY_LOCAL.md    -> Identity.Local provider guide
README_IDENTITY_ASPNET.md   -> Identity.AspNet provider guide
README_TESTING.md           -> shared test helper guide
README_*_TESTS.md           -> focused test coverage summaries
```

## Build and test

From the repository root:

```bash
dotnet restore ArchFramework.slnx
dotnet build ArchFramework.slnx
dotnet test ArchFramework.slnx
```
