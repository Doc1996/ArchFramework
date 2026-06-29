# Leva.Framework.Fakes

`Leva.Framework.Fakes` provides reusable test doubles for Core, Engine, and provider-neutral Identity libraries. It helps tests assemble deterministic runtime scenarios without duplicating production engine behavior.

## Purpose and dependencies

Fakes exist for unit tests, integration-style framework tests, demos, and application tests that need predictable clocks, queues, events, logs, transitions, states, routines, behaviors, status updates, alarms, identity sources, authentication policies, authorization policies, or a ready-made fake context. `Leva.Framework.Fakes` may depend on Core, Engine, and Identity because it is a testing support library for those projects. Engine and Identity must never depend on Fakes. Fakes should not depend on application projects, UI providers, storage providers, notification providers, concrete authentication providers, or external infrastructure.

```text
Leva.Framework.Fakes
  -> Leva.Framework.Engine
  -> Leva.Framework.Identity
  -> Leva.Framework.Core

Leva.Framework.Engine
Leva.Framework.Identity
  -> Leva.Framework.Core
```

## Project overview

Fakes are intentionally small and observable. They expose what happened in a test: captured log entries, transition requests, queued events, current fake time, handler calls, enter/exit counts, status updates, alarm calls, identity/session lookups, policy calls, and configured return behavior.

The source files are grouped by the framework area they support: `Core`, `Engine`, and `Identity`. The namespace stays `Leva.Framework.Fakes` so tests can import one namespace and use all fakes.

Use focused fakes when testing one component. For example, use `FakeClock` for time-dependent tests, `FakeTransition` for transition-request tests, `FakeLogSink` for log-output tests, and `FakeEventQueue` for queue behavior. Use `FakeContext` when a test needs convenient fake wiring and manual composition would distract from the test purpose.

Fakes should not become a second engine. If a fake starts reproducing too much production behavior, the test should probably use Engine directly or introduce a smaller test seam.

## Files and classes

### Core fakes

`FakeAccess` - Simple access object for tests that exposes transition and log capabilities.
`FakeClock` - Controllable clock for deterministic tests.
`FakeEvent` - Simple event implementation with generated or supplied `EventId` and name.
`FakeEventQueue` - Deterministic in-memory event queue implementation with priority ordering and async waiting.
`FakeLogSink` - Stores log entries in memory for tests.
`FakeTransition` - In-memory transition capability that captures requested transitions.
`FakeTransitionEntry` - Represents one transition request captured by `FakeTransition`.

### Engine fakes

`FakeAlarmSupervisor` - Configurable `IAlarmSupervisor` fake with call tracking.
`FakeBehavior` - Configurable behavior fake that records handling calls.
`FakeContext` - Provides ready-to-use fake clock, log sink, transition, access, and event queue wiring for tests.
`FakeRoutine` - Configurable routine fake with mutable lifecycle status.
`FakeState` - Configurable state fake that records enter, exit, and handle calls.
`FakeStatusUpdater` - Configurable `IStatusUpdater` fake with call tracking.

### Identity fakes

`FakePrincipalStore` - In-memory principal store fake for tests.
`FakePrincipalSessionStore` - In-memory principal session store fake for tests.
`FakePrincipalSessionSource` - Configurable current principal session source fake for tests.
`FakeAuthenticationPolicy` - Configurable authentication policy fake with call tracking.
`FakeAuthorizationPolicy` - Configurable authorization policy fake with call tracking and simple policy matching.
