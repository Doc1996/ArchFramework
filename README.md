# Project Guide

This document describes the current `WhatsThePlan` solution as a human-facing project guide. It explains the purpose of the application, the reusable framework underneath it, the project structure, dependency rules, naming decisions, local build setup, and the current implementation status.

## 1. Project purpose

`WhatsThePlan` is a collaborative planning application built on top of a reusable C#/.NET application framework. The app should eventually support the full planning lifecycle: creating plans, inviting participants, collecting date/time and place options, voting, choosing the final decision, assigning tasks, tracking expenses, sending reminders, adding notes and updates, and supporting groups, templates, invite links, authentication, in-memory storage, SQLite storage, and a Blazor web UI.

The framework below the app is intentionally reusable. It should be useful not only for `WhatsThePlan`, but also for future applications, including hardware/device-oriented apps.

## 2. Architecture rules

The most important architectural rule is strict dependency direction.

```text
Framework stays app-independent.
App code depends on framework.
Domain stays pure.
States and routines receive Scope, never full Context.
Concrete providers live outside domain/workflows.
```

`Framework.*` projects must never reference `WhatsThePlan.*`. The domain project contains only pure domain concepts and depends only on `Framework.Core`. Workflow projects define state/routine behavior and depend on the domain plus framework core contracts. Concrete provider projects implement contracts from the inner layers. Blazor belongs only in Blazor projects, and EF Core/SQLite belongs only in storage provider projects.

The runtime flow is:

```text
EventQueue
-> EventLoop
-> EventDispatcher
-> Monitor
-> Supervisor
-> State
-> Routine
-> Behaviors
-> UnhandledBehavior
-> transition drain
```

`Context` owns the system composition, but states and routines should only receive a narrow `Scope`. A scope exposes only the allowed bundles for that state or routine, such as views, data, notifications, behaviors, calendar, time, transitions, and trace.

## 3. Repository layout

```text
WhatsThePlan.sln

global.json
Directory.Build.props
Directory.Packages.props
.editorconfig
.csharpierrc.json
.vscode/settings.json

PROJECT_GUIDE.md

src/
  Framework.Core/
  Framework.Engine/
  Framework.Auth/
  Framework.Devices/
  Framework.Storage/
  Framework.Storage.InMemory/
  Framework.Storage.Sqlite/
  Framework.Notifications/
  Framework.Notifications.InMemory/
  Framework.Diagnostics/
  Framework.Presentation/
  Framework.Presentation.Blazor/
  Framework.Fakes/

  WhatsThePlan.Domain/
  WhatsThePlan.Workflows/
  WhatsThePlan.Auth/
  WhatsThePlan.Storage/
  WhatsThePlan.Storage.InMemory/
  WhatsThePlan.Storage.Sqlite/
  WhatsThePlan.Notifications/
  WhatsThePlan.Presentation/
  WhatsThePlan.Presentation.Blazor/

tests/
  Framework.*.Tests/
  WhatsThePlan.*.Tests/
```

## 4. Shared repository configuration

`global.json` pins the .NET SDK version for the repository.

`Directory.Build.props` keeps common MSBuild settings in one place so individual `.csproj` files stay smaller and consistent.

```xml
<Project>
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
	</PropertyGroup>
</Project>
```

`Directory.Packages.props` centralizes NuGet versions. Project files should reference packages without repeating versions.

```xml
<Project>
	<PropertyGroup>
		<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
	</PropertyGroup>

	<ItemGroup>
		<PackageVersion Include="Microsoft.Data.Sqlite" Version="10.0.0" />
		<PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
		<PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
		<PackageVersion Include="xunit" Version="2.9.2" />
		<PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
	</ItemGroup>
</Project>
```

Example package reference inside a `.csproj`:

```xml
<PackageReference Include="xunit" />
```

CSharpier is configured with tabs and LF line endings:

```json
{
	"useTabs": true,
	"endOfLine": "lf"
}
```

The `.editorconfig` should apply tabs to C#, project files, props, targets, and XML.

```ini
root = true

[*.{cs,csproj,props,targets,xml}]
indent_style = tab
indent_size = 4
tab_width = 4
```

## 5. Framework projects

`Framework.Core` contains the core vocabulary: results, errors, typed IDs, events, states, scopes, routines, behaviors, transitions, trace, and clock contracts. It must not contain engine logic, storage providers, UI, or app-specific concepts.

`Framework.Engine` contains the runtime implementation: context, event queue, event scheduler, event loop, dispatcher, state machine, transition sink, routine runner, behavior runner, command tracker, system clock, trace sinks, monitor/supervisor hooks, and unhandled behavior handling. It supports `OnExit`, `OnEnter`, `Reenter`, transition-chain draining, and a guard against infinite transition loops.

`Framework.Auth` contains generic user/account/session/permission concepts and contracts such as `IUserContext`, `IAuthorizer`, `IAccountStore`, and `ISessionStore`. The project is named `Auth`, not `Identity`, because `Identity` can be confused with object IDs.

`Framework.Devices` contains generic device concepts for future hardware applications: device IDs, statuses, connection/health states, commands, events, controller contracts, status stores, registries, and `DeviceMonitor`. It should stay generic and must not include robot-, PLC-, or camera-specific APIs.

`Framework.Storage` contains generic storage contracts, while `Framework.Storage.InMemory` and `Framework.Storage.Sqlite` provide reusable in-memory and generic SQLite implementations. App-specific SQLite persistence belongs in app provider projects, not in the generic framework provider.

`Framework.Notifications` contains generic notice, recipient, channel, priority, notifier, scheduler, null notifier, and composite notifier concepts. `Framework.Notifications.InMemory` provides the in-memory implementation.

`Framework.Diagnostics` contains diagnostics entries, metrics, sinks, and reporters. It should not be renamed to `Metrics`, because it includes more than numeric metrics.

`Framework.Presentation` contains UI-agnostic presentation contracts such as navigation, prompts, view registry, event publishing, and refresh signaling. `Framework.Presentation.Blazor` contains generic Blazor adapters, separated so other UI providers can be added later.

`Framework.Fakes` contains reusable fake implementations for tests and controlled scenarios. It replaced the earlier `Framework.Testing` name.

## 6. WhatsThePlan projects

`WhatsThePlan.Domain` is the pure domain model. It contains plans, participants, scheduling, places, tasks, expenses, reminders, notes, groups/templates, events, and summaries. It must not depend on UI, storage provider implementation, EF Core, SQLite, Blazor, notification providers, or workflow runtime concerns.

`WhatsThePlan.Workflows` contains application behavior over time. It is organized around `Contracts`, `Scopes`, `States`, `Routines`, `Behaviors`, and `Events`. It uses `Scopes`, not `Access`, and `Contracts`, not `Ports`, `Services`, or `Capabilities`. Workflow events are input/request events, while domain events are facts that happened.

The workflow states are:

```text
DraftPlanState
InvitingParticipantsState
CollectingOptionsState
ChoosingFinalPlanState
PreparingPlanState
ReadyState
CompletedState
CancelledState
```

`WhatsThePlan.Auth` contains app-specific account profiles, plan roles, plan actions, plan authorization, invite links, and in-memory auth stores.

`WhatsThePlan.Storage` contains provider-neutral app storage contracts/helpers. The concrete providers are `WhatsThePlan.Storage.InMemory` and `WhatsThePlan.Storage.Sqlite`. The SQLite provider uses EF Core SQLite and contains app-specific records and mappers.

`WhatsThePlan.Notifications` contains app-specific invitation and reminder notice builders and senders over `Framework.Notifications`.

`WhatsThePlan.Presentation` is the UI-agnostic app presentation layer. It contains models, mapping, and `PlanAppService`. Blazor should call `PlanAppService` instead of directly mutating domain objects.

`WhatsThePlan.Presentation.Blazor` is the actual Blazor web app provider. The current prototype uses in-memory storage, no login UI, no external notifications, interactive server style, and a feature-based folder structure.

```text
Program.cs
App/
Layouts/
Features/
  Home/
  Plans/
  Participants/
  Scheduling/
  Tasks/
  Expenses/
  Reminders/
  Notes/
Shared/
wwwroot/
```

`Pages` are route-owning screens, `Components` are feature display/control UI, `Forms` are feature input UI, `Shared` is for generic reusable UI, and `wwwroot` remains the ASP.NET static-file convention.

Current routes:

```text
/
/plans
/plans/create
/plans/{planId}
/plans/{planId}/participants
/plans/{planId}/scheduling
/plans/{planId}/tasks
/plans/{planId}/expenses
/plans/{planId}/reminders
/plans/{planId}/notes
```

## 7. Dependency direction

Framework projects are app-independent. `WhatsThePlan.Domain` depends only on `Framework.Core`. `WhatsThePlan.Workflows` depends on the domain and `Framework.Core`. Provider libraries depend inward on domain/workflow contracts and relevant framework provider or contract libraries. `WhatsThePlan.Presentation` depends on domain/workflows/core. `WhatsThePlan.Presentation.Blazor` depends on presentation plus the runtime providers needed for the in-memory prototype.

Framework reference graph:

```text
Framework.Core
  no project references

Framework.Engine
  -> Framework.Core

Framework.Auth
  no project references

Framework.Devices
  -> Framework.Core
  -> Framework.Engine

Framework.Storage
  no project references

Framework.Storage.InMemory
  -> Framework.Storage

Framework.Storage.Sqlite
  -> Framework.Storage
  Package: Microsoft.Data.Sqlite

Framework.Notifications
  no project references

Framework.Notifications.InMemory
  -> Framework.Notifications

Framework.Diagnostics
  no project references

Framework.Presentation
  -> Framework.Core

Framework.Presentation.Blazor
  -> Framework.Presentation
  -> Framework.Engine
  FrameworkReference: Microsoft.AspNetCore.App

Framework.Fakes
  -> Framework.Auth
  -> Framework.Core
  -> Framework.Engine
```

WhatsThePlan reference graph:

```text
WhatsThePlan.Domain
  -> Framework.Core

WhatsThePlan.Workflows
  -> Framework.Core
  -> WhatsThePlan.Domain

WhatsThePlan.Auth
  -> Framework.Auth
  -> WhatsThePlan.Domain

WhatsThePlan.Storage
  -> Framework.Core
  -> Framework.Storage
  -> WhatsThePlan.Domain
  -> WhatsThePlan.Workflows

WhatsThePlan.Storage.InMemory
  -> Framework.Storage.InMemory
  -> WhatsThePlan.Domain
  -> WhatsThePlan.Storage

WhatsThePlan.Storage.Sqlite
  -> Framework.Core
  -> WhatsThePlan.Domain
  -> WhatsThePlan.Storage
  -> WhatsThePlan.Workflows
  Package: Microsoft.EntityFrameworkCore.Sqlite

WhatsThePlan.Notifications
  -> Framework.Core
  -> Framework.Notifications
  -> WhatsThePlan.Domain
  -> WhatsThePlan.Workflows

WhatsThePlan.Presentation
  -> Framework.Core
  -> WhatsThePlan.Domain
  -> WhatsThePlan.Workflows

WhatsThePlan.Presentation.Blazor
  -> Framework.Auth
  -> Framework.Core
  -> Framework.Engine
  -> Framework.Notifications
  -> WhatsThePlan.Domain
  -> WhatsThePlan.Presentation
  -> WhatsThePlan.Storage.InMemory
  -> WhatsThePlan.Workflows
```

## 8. Naming decisions

Use `Scope` for the allowed world of a state or routine. Use `Contracts` for workflow-required interfaces. Use `Monitor` for the event-route hook that updates latest status/facts. Use `Supervisor` for cross-cutting blocking, recovery, or alarm behavior. Use `Behavior` for reusable default event reactions. Use `Auth` for user/session/permission libraries. Use `Fakes` for reusable fake implementations. Use `PlanAppService` for the UI-facing presentation facade.

Avoid `Access`, `Capabilities`, workflow `Services` folders, `Ports`, `Identity` as a library name, `Testing` as the fake-library name, `StatusUpdater`, and `PlanningAppService`.

## 9. Testing

Use xUnit. Test projects live under `tests/`. `Framework.Fakes` is a reusable fake library, not a test project.

The most important test areas are engine behavior, transition handling, storage providers, notifications, auth, diagnostics, presentation adapters, device monitoring, WTP domain invariants, workflows, storage, notifications, presentation mapping, `PlanAppService`, Blazor route constants, and DI wiring.

## 10. Useful commands

```bash
# clean everything
dotnet clean

# restore packages
dotnet restore

# build solution
dotnet build

# run all tests
dotnet test

# run Blazor prototype
dotnet run --project src/WhatsThePlan.Presentation.Blazor

# list solution projects
dotnet sln list

# list references for one project
dotnet list src/WhatsThePlan.Presentation.Blazor reference

# list packages for one project
dotnet list src/WhatsThePlan.Storage.Sqlite package
```

## 11. Current status

The project is ready for local build, tests, and code review. The architecture and project split are broad and clean, but the generated code still needs real local validation.

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

The first expected issues are most likely Razor syntax, EF Core mapping, small type/reference mismatches, or test API mismatches. Do not add more architecture until the current solution builds and tests locally.
