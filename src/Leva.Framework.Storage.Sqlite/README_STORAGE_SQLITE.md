# Leva.Framework.Storage.Sqlite

`Leva.Framework.Storage.Sqlite` provides local SQLite implementations of the provider-neutral contracts from `Leva.Framework.Storage`.

## Purpose and dependencies

SQLite exists for small durable applications, local tools, desktop-style hosts, and simple application persistence without running a database server. It depends on `Leva.Framework.Storage`, `Leva.Framework.Core`, and `Microsoft.Data.Sqlite`; it uses JSON serialization for stored values and SQLite columns for provider-owned metadata.

```text
Leva.Framework.Storage.Sqlite
  -> Leva.Framework.Storage
  -> Leva.Framework.Core
  -> Microsoft.Data.Sqlite
  -> .NET base libraries
```

## Project overview

The provider stores repository records and journal entries in one SQLite database file. Repositories store keyed values in a repository table partition. Journals store ordered append-only entries in a journal table partition using provider-assigned sequence versions.

Storage sessions use one SQLite connection and transaction. Commit commits the transaction, while rollback or disposal abandons it.

The public repository and journal classes stay thin. They delegate storage behavior to internal stores, while `SqliteStorageDatabase` owns connection creation, serialization, key conversion, and schema initialization.

## Files and classes

### Provider entry point

`SqliteStorageProvider` - Creates repositories, journals, and storage sessions for one SQLite database file.
`SqliteStorageDatabase` - Owns database paths, connections, serialization, and key conversion for one provider instance.
`SqliteStorageInitializer` - Creates the provider-owned SQLite schema when a database connection is opened.

### Repository implementation

`SqliteRepository<TId, TModel>` - Stores keyed models in SQLite under a named repository.
`SqliteRepositoryStore<TId, TModel>` - Holds SQL operations for one named repository table partition.

### Journal implementation

`SqliteJournal<TEntry>` - Stores append-only journal entries in SQLite under a named journal.
`SqliteJournalStore<TEntry>` - Holds SQL operations for one named append-only journal table partition.

### Session implementation

`SqliteStorageSession` - Owns a SQLite connection and transaction until commit or rollback.
