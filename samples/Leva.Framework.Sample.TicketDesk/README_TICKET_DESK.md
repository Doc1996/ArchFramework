# Leva.Framework.Sample.TicketDesk

Web sample for Storage, Identity.Memory, and Notifications.Memory. It runs the same ticket flow with memory, file, and SQLite storage providers.

```text
Leva.Framework.Sample.TicketDesk
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

## Wiring

```text
Program.cs
  -> maps POST /ticketdesk/run
  -> calls TicketDeskRunner.RunAsync()

TicketDeskRunner
  -> creates MemoryPrincipalServices and logs in a sample agent
  -> creates MemoryNotificationServices
  -> creates MemoryStorageProvider, FileStorageProvider, and SqliteStorageProvider
  -> asks each provider for IRepository<string, Ticket> and IJournal<TicketActivity>
  -> runs the same TicketDeskApp against each provider

TicketDeskApp
  -> checks PrincipalAccess.RequireAsync("tickets.manage")
  -> saves Ticket through IRepository
  -> appends TicketActivity through IJournal
  -> sends assignment notification through NotificationService
```

## Files

`TicketDeskRunner.cs` - Provider setup and scenario comparison.
`TicketDeskApp.cs` - Provider-neutral application use case.
`Ticket.cs`, `TicketActivity.cs`, `TicketStatus.cs` - Sample domain model.
`TicketDeskResult.cs`, `TicketDeskScenarioResult.cs`, `TicketDeskCheck.cs` - Browser-facing result DTOs.
`wwwroot/` - Browser page, CSS, and JavaScript output rendering.

## Run

```bash
dotnet run --project samples/Leva.Framework.Sample.TicketDesk
```

Expected result:

```text
PASS: identity login created active session
PASS: all storage scenarios passed
PASS: one notification was sent per scenario
PASS: one notification entry was stored per scenario
```
