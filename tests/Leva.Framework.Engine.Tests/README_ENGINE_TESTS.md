# Leva.Framework.Engine.Tests

`Leva.Framework.Engine.Tests` verifies the runtime behavior of `Leva.Framework.Engine` using `Leva.Framework.Fakes`. These tests cover boards, logs, queueing, dispatch, state transitions, routines, and snapshot restore behavior.

## Coverage

`BoardTests` - Verifies basic alarm, status, and command board behavior.
`BoardLifecycleTests` - Verifies board clear, clear-all, duplicate command, set, cancel, and timeout behavior.
`RuntimeLogTests` - Verifies structured runtime logging and log mirroring.
`RuntimeLogDetailTests` - Verifies dictionary merge, ID normalization, explicit log levels, and log clearing.
`EventQueueTests` - Verifies priority dequeueing, delayed enqueueing, and delayed cancellation.
`EventQueueLifecycleTests` - Verifies queue waiting, cancellation, duplicate delayed events, external cancellation, and delayed priority.
`StateMachineTests` - Verifies state entry, handling, and transitions.
`StateMachineTransitionTests` - Verifies reentry, routine cancellation during transition, transition loop guard, and double-start protection.
`DispatcherAndRoutineTests` - Verifies behavior fallback, alarm handling, and routine completion.
`DispatcherOrderTests` - Verifies deterministic dispatcher ordering, behavior-requested transitions, and unhandled logging.
`DispatcherShortCircuitTests` - Verifies dispatcher short-circuit rules when alarms, states, or routines handle events.
`SnapshotTests` - Verifies snapshot creation and loading of board snapshots.
`SnapshotEdgeTests` - Verifies snapshot pre-start guard, single-item restore, and replacement of previous board entries.
