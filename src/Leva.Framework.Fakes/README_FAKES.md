# Leva.Framework.Fakes

`Leva.Framework.Fakes` provides reusable test doubles for Core, Engine, and provider-neutral framework libraries. It helps tests assemble deterministic runtime scenarios without duplicating production engine behavior.

## Purpose and dependencies

Fakes exist for unit tests, integration-style framework tests, demos, and application tests that need predictable clocks, queues, events, logs, transitions, states, routines, behaviors, status updates, alarms, principal sources, authentication policies, authorization policies, or a ready-made fake context. `Leva.Framework.Fakes` may depend on Core, Engine, and Identity because it is a testing support library for those projects. Engine and Identity must never depend on Fakes. Fakes should not depend on application projects, UI providers, storage providers, notification providers, concrete authentication providers, or external infrastructure.

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

Fakes are intentionally small and observable. They expose what happened in a test: captured log entries, transition requests, queued events, current fake time, handler calls, enter/exit counts, status updates, alarm calls, principal/session lookups, policy calls, and configured return behavior.

Use focused fakes when testing one component. For example, use `FakeClock` for time-dependent tests, `FakeTransition` for transition-request tests, `FakeLogSink` for log-output tests, and `FakeEventQueue` for queue behavior. Use `FakeContext` when a test needs convenient fake wiring and manual composition would distract from the test purpose.

Fakes should not become a second engine. If a fake starts reproducing too much production behavior, the test should probably use Engine directly or introduce a smaller test seam.

## Files and classes

### Access and transitions

`FakeAccess` - Simple access object for tests that exposes transition and log capabilities.
`FakeTransition` - In-memory transition capability that captures requested transitions.
`FakeTransitionEntry` - Represents one transition request captured by `FakeTransition`.

### Runtime fakes

`FakeClock` - Controllable clock for deterministic tests.
`FakeEvent` - Simple event implementation with generated or supplied `EventId` and name.
`FakeEventQueue` - Deterministic in-memory event queue implementation with priority ordering and async waiting.
`FakeLogSink` - Stores log entries in memory for tests.
`FakeAlarmSupervisor` - Configurable `IAlarmSupervisor` fake with call tracking.
`FakeStatusUpdater` - Configurable `IStatusUpdater` fake with call tracking.

### State, routine, and behavior fakes

`FakeState` - Configurable state fake that records enter, exit, and handle calls.
`FakeRoutine` - Configurable routine fake with mutable lifecycle status.
`FakeBehavior` - Configurable behavior fake that records handling calls.

### Context helper

`FakeContext` - Provides ready-to-use fake clock, log sink, transition, access, and event queue wiring for tests.

### Identity fakes

`FakePrincipalStore` - In-memory principal store fake for tests.
`FakePrincipalSessionStore` - In-memory principal session store fake for tests.
`FakePrincipalSessionSource` - Configurable current principal session source fake for tests.
`FakeAuthenticationPolicy` - Configurable authentication policy fake with call tracking.
`FakeAuthorizationPolicy` - Configurable authorization policy fake with call tracking and simple policy matching.
