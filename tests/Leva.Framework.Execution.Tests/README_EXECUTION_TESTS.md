# Leva.Framework.Execution.Tests

`Leva.Framework.Execution.Tests` verifies the background execution runtime. These tests keep execution tracking, registration, progress, cancellation, failure handling, and typed result completion aligned with the public Execution API.

## Coverage

`ExecutionBoardTests` - Verifies entry tracking through the runner, latest progress, completed and failed entries, clearing, logging through `FakeLogSink`, and registered names.
`ExecutionRunnerTests` - Verifies direct execution, board-registered execution, unregistered execution failure, typed result completion, cancellation through `ExecutionHandle`, failed result tracking, and exception-to-failure mapping.
