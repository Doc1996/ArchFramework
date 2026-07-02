# Samples

This folder is reserved for small runnable applications that demonstrate how the framework libraries compose in real code. Samples are intentionally minimal and are not production applications.

```text
Leva.Framework.Sample.CounterWorkflow   Core, Engine, Execution, Fakes
Leva.Framework.Sample.TicketDesk        Storage, Storage.Memory, Storage.Sqlite, Identity.Memory, Notifications.Memory
Leva.Framework.Sample.LiveDashboard     Notifications.SignalR, minimal live web UI
Leva.Framework.Sample.LocalAccount      Identity.Local, Identity.AspNet, local provider setup
```

Samples do not have separate test projects for now. If a sample reveals missing behavior, the corresponding framework library should receive the test.
