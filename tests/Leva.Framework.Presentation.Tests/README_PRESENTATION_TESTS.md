# Leva.Framework.Presentation.Tests

`Leva.Framework.Presentation.Tests` verifies the provider-neutral presentation state values and host-facing contracts from `Leva.Framework.Presentation`. These tests cover messages, view state, command state, form state, field state, dialogs, message sinks, and navigation.

## Coverage

`MessageEntryTests` - Verifies presentation messages, message levels, titles, codes, targets, and conversion from Core errors.
`ViewStateTests` - Verifies idle, loading, ready, empty, and failed view states.
`CommandStateTests` - Verifies ready, running, succeeded, failed, and disabled command states.
`FormStateTests` - Verifies clean, modified, submitting, submitted, invalid, and failed form states.
`FieldStateTests` - Verifies field values, touched and modified flags, field updates, and targeted field messages.
`DialogTests` - Verifies dialog requests, cancel detection, accepted results, and cancelled results.
`MessageSinkTests` - Verifies memory and null message sink behavior.
`NavigationTests` - Verifies host navigation capability implementations.
