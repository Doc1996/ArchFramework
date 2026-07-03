# Samples

The samples are small runnable ASP.NET Core applications that show how the reusable framework libraries are wired in real hosts. They are not exhaustive verification suites. They show the main usage path with visible browser feedback, while detailed behavior verification belongs in library test projects.

Each sample follows the same shape:

```text
Program.cs       -> ASP.NET Core host, service setup, endpoint mapping
*.cs             -> sample domain/use-case code and framework wiring
wwwroot/         -> static page, CSS, JavaScript checks
README_*.md      -> compact wiring notes and expected behavior
```

Static files are served with no-cache headers so browser changes are visible during development.

## Applications

`Leva.Framework.Sample.StateCounter` - Shows Core, Engine, Execution, and Fakes through states, events, delayed events, behavior, alarms, statuses, runtime log, and execution report.
`Leva.Framework.Sample.StorageDesk` - Shows Storage.Memory, Storage.Files, Storage.Sqlite, Identity.Memory, and Notifications.Memory through the same ticket workflow.
`Leva.Framework.Sample.LiveDashboard` - Shows Notifications.SignalR with a browser SignalR client and memory-backed notification history.
`Leva.Framework.Sample.LocalIdentity` - Shows Identity.Local and Identity.AspNet through local credentials, auth-session cookie, and framework permission authorization.
`Leva.Framework.Sample.WebIdentity` - Shows Identity.AspNet JWT bearer tokens and optional Google OAuth sign-in.

## Running

Run one sample:

```bash
dotnet run --project samples/Leva.Framework.Sample.StateCounter
```

Run all samples on separate ports:

```bash
chmod +x samples/run-all.sh
./samples/run-all.sh
```

Open the printed local URL and use the page buttons.

## Library coverage

```text
Core                       -> StateCounter, StorageDesk, LiveDashboard, LocalIdentity, WebIdentity
Engine                     -> StateCounter
Execution                  -> StateCounter
Fakes                      -> StateCounter
Storage                    -> StorageDesk
Storage.Memory             -> StorageDesk
Storage.Files              -> StorageDesk
Storage.Sqlite             -> StorageDesk
Identity                   -> StorageDesk, LocalIdentity, WebIdentity
Identity.Memory            -> StorageDesk, LocalIdentity, WebIdentity
Identity.Local             -> LocalIdentity, WebIdentity
Identity.AspNet            -> LocalIdentity, WebIdentity
Notifications              -> StorageDesk, LiveDashboard
Notifications.Memory       -> StorageDesk, LiveDashboard
Notifications.SignalR      -> LiveDashboard
Microsoft.AspNetCore       -> all samples as minimal web hosts
```
