# Leva.Framework.Sample.LiveDashboard

Web sample for Notifications.SignalR with a memory-backed visible history. It sends framework notifications to the browser in real time and shows stored notification entries separately.

```text
Leva.Framework.Sample.LiveDashboard
  -> Leva.Framework.Core
  -> Leva.Framework.Notifications
  -> Leva.Framework.Notifications.Memory
  -> Leva.Framework.Notifications.SignalR
  -> Microsoft.AspNetCore
```

## Setup shown by the sample

```text
Program.cs
  -> registers QueryStringUserIdProvider for demo user mapping
  -> calls AddSignalRNotifications() for the SignalR gateway and hub
  -> registers MemoryNotificationStore as the sample history store
  -> registers NotificationService explicitly
  -> maps MapSignalRNotifications()
  -> maps dashboard endpoints

DashboardNotificationHistory
  -> reads the concrete MemoryNotificationStore used by the page
  -> exposes small dashboard DTOs instead of raw framework objects
```

## Behavior shown

```text
Browser connects as user demo
POST /dashboard/notify -> NotificationService -> SignalR gateway -> browser receives message
MemoryNotificationStore -> stored history shown by Refresh stored history
```

The browser has two separate views:

```text
Live notifications -> messages received through SignalR in the current browser tab
Stored history     -> notification entries stored on the server
```

**Clear browser view** only clears the browser display. It does not clear the server-side memory store. **Refresh stored history** reads the stored server history again and re-renders the history list. The sample host sets a short shutdown timeout so Ctrl+C stops quickly even with an open SignalR connection.

## Run

```bash
dotnet run --project samples/Leva.Framework.Sample.LiveDashboard
```

## Expected result

```text
Connected to SignalR as user demo.
Send notification -> browser receives live notification
Refresh stored history -> stored count is updated
Self-check -> PASS
```
