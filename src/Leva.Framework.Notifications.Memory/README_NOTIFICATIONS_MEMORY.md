# Leva.Framework.Notifications.Memory

`Leva.Framework.Notifications.Memory` is the in-process notification provider for ArchFramework. It provides a memory gateway, memory store, and composition helper for development, samples, local demos, and simple tests.

## Purpose and dependencies

Notifications.Memory exists so applications and tests can use the provider-neutral notification contracts without configuring ASP.NET, email, push, SignalR, or other external delivery infrastructure. It is a real in-memory provider: sends succeed and are recorded in memory. Test-specific failure behavior belongs in `Leva.Framework.Fakes`, not in this provider.

`Leva.Framework.Notifications.Memory` depends on `Leva.Framework.Notifications` and `Leva.Framework.Core`. It does not depend on Engine, Storage, Identity, ASP.NET, email providers, push providers, or application projects.

```text
Leva.Framework.Notifications.Memory
  -> Leva.Framework.Notifications
  -> Leva.Framework.Core
  -> .NET base libraries

Leva.Framework.Notifications
  -> Leva.Framework.Core
```

## Project overview

The memory provider keeps all notification state in process. `MemoryNotificationGateway` records notifications sent through the gateway and returns successful results. `MemoryNotificationStore` stores notification entries by `NotificationId`. Both use the synchronized collection helpers from Core, so snapshots are safe to read while multiple sends or store operations happen concurrently.

`MemoryNotificationServices` groups the memory gateway, memory store, and `NotificationService` for convenient composition. It follows the same service-group pattern as the memory and local identity libraries.

## Files and classes

### Memory gateway

`MemoryNotificationGateway` - Implements `INotificationGateway`, records sent notifications through a `SyncList<Notification>`, exposes a snapshot through `Sent`, and supports clearing in-memory state.

### Memory store

`MemoryNotificationStore` - Implements `INotificationStore`, stores notification entries through a `SyncDictionary<NotificationId, NotificationEntry>`, exposes snapshots through `Entries`, and supports save, load, list, delete, and clear operations.

### Service group

`MemoryNotificationServices` - Creates and exposes the memory gateway, memory store, and `NotificationService`, using `SystemClock` by default unless an `IClock` is provided.
