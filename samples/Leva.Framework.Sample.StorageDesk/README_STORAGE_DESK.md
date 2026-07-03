# Leva.Framework.Sample.StorageDesk

Web sample for Storage, Identity.Memory, and Notifications.Memory. It runs the same ticket flow against memory, file, and SQLite storage providers.

```text
Leva.Framework.Sample.StorageDesk
  -> Leva.Framework.Core
  -> Leva.Framework.Storage
  -> Leva.Framework.Storage.Memory
  -> Leva.Framework.Storage.Files
  -> Leva.Framework.Storage.Sqlite
  -> Leva.Framework.Identity
  -> Leva.Framework.Identity.Memory
  -> Leva.Framework.Notifications
  -> Leva.Framework.Notifications.Memory
  -> Microsoft.AspNetCore
```

## Setup shown by the sample

```text
StorageDeskRunner
  -> creates MemoryPrincipalServices
  -> creates MemoryNotificationServices
  -> logs in a manager principal
  -> creates memory, file, and SQLite storage providers
  -> passes IRepository<string, Ticket> and IJournal<TicketActivity> to StorageDeskApp

StorageDeskApp
  -> uses PrincipalAccess to require tickets.manage
  -> saves Ticket through IRepository
  -> appends TicketActivity through IJournal
  -> sends assignment and resolution notifications through NotificationService
```

## Behavior shown

```text
New ticket      -> Assigned ticket       -> Resolved ticket
Repository save -> Journal activity      -> Notification entry
memory          -> files                 -> sqlite
```

Each storage provider runs the same application flow. The sample checks that the ticket is saved, two activity entries are written, and two notification entries are stored.

## Run and expected result

```bash
dotnet run --project samples/Leva.Framework.Sample.StorageDesk
```

```text
memory: PASS
files: PASS
sqlite: PASS
two activity entries per provider
two notification entries per provider
Sample result: PASS
```
