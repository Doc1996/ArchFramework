# Framework.Core.Tests

`Framework.Core.Tests` verifies the stable contracts and value types from `Framework.Core`. These tests focus on IDs, entries, result types, snapshots, and interface implementability rather than engine behavior.

## Coverage

`IdTests` - Verifies generated runtime IDs and named IDs.
`CoreValueTests` - Verifies priority ordering, default runtime IDs, and entry properties.
`EntryTests` - Verifies alarm, command, status, trace, and snapshot records.
`ResultTests` - Verifies success and failure result values.
`ContractTests` - Verifies that the basic Core interfaces can be implemented and used together.
