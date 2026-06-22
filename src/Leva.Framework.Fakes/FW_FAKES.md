# Framework.Fakes

`Framework.Fakes` provides small reusable test doubles for `Framework.Core` and `Framework.Engine`. The fakes are intended for unit tests, demos, and examples; they should stay simple and should not become a second engine implementation.

## Design rules

Fakes expose observable state such as call counts, captured transition entries, trace entries, and queued events. They should follow the same public API names as Core and Engine, especially typed entry collections such as `TraceEntries` and `TransitionEntries`.

Use `FakeContext` when a test needs convenient fake runtime wiring. Use individual fakes when testing one class in isolation.

## Types

`FakeAccess` - Provides fake-compatible transition and trace capabilities.
`FakeAlarmSupervisor` - Counts and optionally handles alarm supervision calls.
`FakeBehavior` - Configurable behavior fake with handle count tracking.
`FakeClock` - Provides deterministic runtime time.
`FakeContext` - Provides ready-to-use fake test wiring, with fake helpers and an optional engine context.
`FakeEvent` - Simple event implementation with generated or supplied `EventId`.
`FakeEventQueue` - Deterministic in-memory `IEventQueue` implementation with priority ordering and async waiting.
`FakeRoutine` - Configurable routine fake with mutable lifecycle status.
`FakeState` - Configurable state fake with enter, exit, and handle counters.
`FakeStatusUpdater` - Counts and optionally performs status updates.
`FakeTraceSink` - Stores trace entries in memory.
`FakeTransition` - Captures `ITransition` requests in memory.
`FakeTransitionEntry` - Represents one captured transition request.
