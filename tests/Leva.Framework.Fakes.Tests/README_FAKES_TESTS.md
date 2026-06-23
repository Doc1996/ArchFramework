# Leva.Framework.Fakes.Tests

`Leva.Framework.Fakes.Tests` verifies the reusable fakes used by Core and Engine tests. These tests keep fake behavior aligned with the current Core and Engine APIs.

## Coverage

`FakeClockTests` - Verifies deterministic time control.
`FakeTransitionTests` - Verifies captured transition requests and clearing.
`FakeLogSinkTests` - Verifies log entry capture and clearing.
`FakeEventQueueTests` - Verifies basic deterministic queue behavior.
`FakeEventQueueBehaviorTests` - Verifies priority ordering, async waiting, cancellation, delayed enqueue behavior, and clearing.
`FakeRuntimeTests` - Verifies fake states, routines, behaviors, and complete fake contexts.
`FakeContextTests` - Verifies default fake wiring, provided fake instances, and engine integration.
