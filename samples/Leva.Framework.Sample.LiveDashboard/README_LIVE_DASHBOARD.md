# Leva.Framework.Sample.LiveDashboard

Web sample for Notifications.SignalR, a memory notification store, and ASP.NET Core. The browser connects as SignalR user `demo`, sends framework notifications, receives live messages, and refreshes stored history.

```text
Leva.Framework.Sample.LiveDashboard
  -> Leva.Framework.Core
  -> Leva.Framework.Notifications
  -> Leva.Framework.Notifications.Memory
  -> Leva.Framework.Notifications.SignalR
  -> Microsoft.AspNetCore
```

## Wiring

```text
Program.cs
  -> registers IClock as SystemClock
  -> registers QueryStringUserIdProvider as IUserIdProvider
  -> calls AddSignalRNotifications() for SignalR hub/gateway setup
  -> registers MemoryNotificationStore as INotificationStore after SignalR setup
  -> registers NotificationService using SignalR gateway + memory store
  -> calls MapSignalRNotifications()
  -> maps dashboard endpoints

/dashboard/notify
  -> calls NotificationService.SendAsync(..., SignalRNotificationOptions.DefaultChannel, ...)
  -> SignalRNotificationGateway sends to Clients.User("demo")
  -> NotificationService stores NotificationEntry in MemoryNotificationStore

/dashboard/history
  -> reads the same MemoryNotificationStore instance
  -> returns DashboardNotificationEntry DTOs to the browser
```

## Files

`Program.cs` - SignalR/notification service setup and dashboard endpoints.
`QueryStringUserIdProvider.cs` - Maps `?user=demo` to the SignalR user id.
`DashboardNotificationEntry.cs` - Browser-facing history DTO.
`DashboardSelfCheck.cs` - Browser-facing self-check result.
`wwwroot/` - Browser page, CSS, SignalR client, and JavaScript checks.

## Run

```bash
dotnet run --project samples/Leva.Framework.Sample.LiveDashboard
```

Expected behavior:

```text
Connected to SignalR as user demo.
PASS: server accepted notification send.
PASS: browser received SignalR notification.
PASS: refreshed stored notification entries.
```

The sample host sets a two-second shutdown timeout because active SignalR connections can otherwise make `Ctrl+C` wait. That timeout belongs in the application host, not in the framework provider.
