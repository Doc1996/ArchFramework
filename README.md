# ArchFramework

ArchFramework is a modular .NET framework for building event-driven applications with explicit runtime workflows, provider-neutral storage, identity, notifications, execution tracking, and reusable test fakes.

The repository is organized around small libraries. Each library has a focused README and a local wiring diagram that shows which services to instantiate or inject, which interfaces providers implement, and which methods are normally called.

## Repository structure

```text
src/       Framework libraries and provider implementations
tests/     Unit tests for framework libraries
samples/   Small runnable sample applications
```

## Libraries

```text
Leva.Framework.Core                    Shared contracts, results, IDs, clocks, entries, snapshots, synchronization helpers
Leva.Framework.Engine                  Event queue, dispatcher, state machine, routines, behaviors, boards, runtime log
Leva.Framework.Execution               Execution runner, execution board, entries, status, progress, cancellation
Leva.Framework.Fakes                   Test doubles and shared result assertions

Leva.Framework.Storage                 Provider-neutral repository and journal contracts
Leva.Framework.Storage.Memory          In-process storage provider
Leva.Framework.Storage.Files           Local JSON/file-system storage provider
Leva.Framework.Storage.Sqlite          Local SQLite storage provider

Leva.Framework.Identity                Principals, sessions, authentication, authorization, audit contracts
Leva.Framework.Identity.Memory         In-memory identity provider
Leva.Framework.Identity.Local          Local/offline credential provider
Leva.Framework.Identity.AspNet         ASP.NET Core identity integration

Leva.Framework.Notifications           Provider-neutral notification model, gateway, store, service
Leva.Framework.Notifications.Memory    In-memory notification provider
Leva.Framework.Notifications.SignalR   ASP.NET Core SignalR notification provider
```

## Samples

```text
Leva.Framework.Sample.StateCounter      Core, Engine, Execution, and Fakes in a small state-counter flow
Leva.Framework.Sample.StorageDesk       Storage providers, Identity.Memory, and Notifications.Memory in a ticketing flow
Leva.Framework.Sample.LiveDashboard     Notifications.SignalR in a minimal live web dashboard
Leva.Framework.Sample.LocalIdentity     Identity.Local and Identity.AspNet in a minimal account host
```

## Documentation convention

Each library contains:

```text
README_*.md       Local explanation of the library
DIAGRAM_*.png     Wiring diagram for normal use
```

Diagram colors are consistent across libraries:

```text
Green   Application or tests
Blue    Framework or provider implementation
Yellow  Interface or contract
White   Value, entry, payload, ID, or model
```

Function and method names are written in italics and include `()`. Arrow labels use a small shared vocabulary such as `uses`, `calls`, `creates`, `implements`, `stores`, `returns`, `sends`, and `maps`.

## Build and test

From the repository root:

```bash
dotnet restore ArchFramework.slnx
dotnet build ArchFramework.slnx
dotnet test ArchFramework.slnx
```

Run one sample:

```bash
dotnet run --project samples/Leva.Framework.Sample.StateCounter
```
