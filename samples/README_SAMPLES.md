# Samples

These are small runnable web samples that show how the framework libraries are wired in application code. They are not full verification suites. They show the main usage path with visible browser feedback, while exhaustive behavior belongs in library tests.

Each sample follows the same shape:

```text
Program.cs       -> ASP.NET Core host, service setup, endpoint mapping
*.cs             -> sample domain/use-case code and framework wiring
wwwroot/         -> static page, CSS, JavaScript checks
README_*.md      -> compact wiring notes and expected behavior
```

Static files are served with no-cache headers and versioned links because the samples change often while developing.

## Applications

`Leva.Framework.Sample.CounterWorkflow` - Core, Engine, Execution, and Fakes through a state-machine workflow.
`Leva.Framework.Sample.TicketDesk` - Storage providers, Identity.Memory, and Notifications.Memory through one ticketing flow.
`Leva.Framework.Sample.LiveDashboard` - Notifications.SignalR plus a Memory notification store through a live browser notification page.
`Leva.Framework.Sample.LocalAccount` - Identity.Local, Identity.AspNet, and Identity.Memory through local login, cookie session, and authorization.

## Running

```bash
dotnet run --project samples/Leva.Framework.Sample.CounterWorkflow
dotnet run --project samples/Leva.Framework.Sample.TicketDesk
dotnet run --project samples/Leva.Framework.Sample.LiveDashboard
dotnet run --project samples/Leva.Framework.Sample.LocalAccount
```

Open the printed local URL and use the page buttons.

## Library coverage

```text
Core                       -> CounterWorkflow, TicketDesk, LiveDashboard, LocalAccount
Engine                     -> CounterWorkflow
Execution                  -> CounterWorkflow
Fakes                      -> CounterWorkflow
Storage                    -> TicketDesk
Storage.Memory             -> TicketDesk
Storage.Files              -> TicketDesk
Storage.Sqlite             -> TicketDesk
Identity                   -> TicketDesk, LocalAccount
Identity.Memory            -> TicketDesk, LocalAccount
Identity.Local             -> LocalAccount
Identity.AspNet            -> LocalAccount
Notifications              -> TicketDesk, LiveDashboard
Notifications.Memory       -> TicketDesk, LiveDashboard
Notifications.SignalR      -> LiveDashboard
Microsoft.AspNetCore       -> all samples as minimal web hosts
```
