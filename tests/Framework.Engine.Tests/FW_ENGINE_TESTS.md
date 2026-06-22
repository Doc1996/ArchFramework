# Framework.Engine.Tests

`Framework.Engine.Tests` verifies the runtime behavior of `Framework.Engine` using `Framework.Fakes`. These tests cover boards, logs, queueing, dispatch, state transitions, routines, and snapshot restore behavior.

## Coverage

`BoardTests` - Verifies basic alarm, status, and command board behavior.
`BoardLifecycleTests` - Verifies board clear, clear-all, duplicate command, set, cancel, and timeout behavior.
`RuntimeLogTests` - Verifies structured runtime logging and trace mirroring.
`RuntimeLogDetailTests` - Verifies dictionary merge, ID normalization, explicit trace levels, and log clearing.
`EventQueueTests` - Verifies priority dequeueing, delayed enqueueing, and delayed cancellation.
`EventQueueLifecycleTests` - Verifies queue waiting, cancellation, duplicate delayed events, external cancellation, and delayed priority.
`StateMachineTests` - Verifies state entry, handling, and transitions.
`StateMachineTransitionTests` - Verifies reentry, routine cancellation during transition, transition loop guard, and double-start protection.
`DispatcherAndRoutineTests` - Verifies behavior fallback, alarm handling, and routine completion.
`DispatcherOrderTests` - Verifies deterministic dispatcher ordering, behavior-requested transitions, and unhandled logging.
`SnapshotTests` - Verifies snapshot creation and loading of board snapshots.
`SnapshotEdgeTests` - Verifies snapshot pre-start guard, single-item restore, and replacement of previous board entries.
