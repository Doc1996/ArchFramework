# Leva.Framework.Sample.CounterWorkflow

Web sample for Core, Engine, Execution, and Fakes. It builds a small workflow, queues events, runs the event loop, starts one execution, and shows runtime/execution output in the browser.

```text
Leva.Framework.Sample.CounterWorkflow
  -> Leva.Framework.Core
  -> Leva.Framework.Engine
  -> Leva.Framework.Execution
  -> Leva.Framework.Fakes
  -> Microsoft.AspNetCore
```

## Wiring

```text
Program.cs
  -> maps POST /workflow/run
  -> calls CounterWorkflowRunner.RunAsync()

CounterWorkflowRunner
  -> creates FakeClock and FakeLogSink
  -> creates ExecutionBoard and ExecutionRunner
  -> creates CounterAccess from framework IAccess
  -> registers IdleState, CountingState, CompletedState in ContextBuilder
  -> starts StateMachine at Idle
  -> enqueues StartCounterEvent, IncrementCounterEvent, CompleteCounterEvent
  -> runs EventLoop.RunOneAsync(...) for each queued event
  -> returns RuntimeLog and ExecutionBoard entries to the browser

States
  -> handle IEvent values
  -> call access.Transition.To(...) instead of touching StateMachine directly
  -> use access.Model for sample state
  -> use access.Executions for typed background work
```

## Files

`CounterWorkflowRunner.cs` - Main sample composition and scenario runner.
`CounterAccess.cs` - Application-specific access object passed into states.
`States/` - Engine states that consume events and request transitions.
`Events/` - Small `IEvent` records using `Leva.Framework.Core.EventId`.
`Executions/` - Typed execution request and execution implementation.
`wwwroot/` - Browser page, CSS, and JavaScript output rendering.

## Run

```bash
dotnet run --project samples/Leva.Framework.Sample.CounterWorkflow
```

Expected result:

```text
PASS: workflow reached Completed state
PASS: counter handled two increment events
PASS: execution produced the report
PASS: runtime log captured events, transitions, and states
PASS: execution board captured one execution
```
