# Leva.Framework.Fakes.Tests

`Leva.Framework.Fakes.Tests` verifies the reusable fakes and shared test helpers used by Core, Engine, Identity, Storage, and Notifications tests. These tests keep fake behavior aligned with the current framework APIs.

## Coverage

`FakeClockTests` - Verifies deterministic time control.
`FakeTransitionTests` - Verifies captured transition requests and clearing.
`FakeLogSinkTests` - Verifies log entry capture and clearing.
`FakeEventQueueTests` - Verifies basic deterministic queue behavior.
`FakeEventQueueBehaviorTests` - Verifies priority ordering, async waiting, cancellation, delayed enqueue behavior, and clearing.
`FakeRuntimeTests` - Verifies fake states, routines, behaviors, and complete fake contexts.
`FakeContextTests` - Verifies default fake wiring, provided fake instances, and engine integration.
`FakePrincipalTests` - Verifies principal stores, session stores, session sources, authentication policies, and authorization policies.
`FakeNotificationGatewayTests` - Verifies one-shot notification gateway failure behavior and captured sends.
`ResultAssert` - Shared helper used by framework tests to unwrap successful `Result` and `Result<T>` values.
