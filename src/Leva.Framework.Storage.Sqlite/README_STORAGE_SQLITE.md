# Leva.Framework.Storage.Sqlite

`Leva.Framework.Storage.Sqlite` is the SQLite provider implementation of `Leva.Framework.Storage`. It stores repositories and journals in one SQLite database file.

## Purpose and dependencies

`Leva.Framework.Storage.Sqlite` depends on `Leva.Framework.Storage`, `Leva.Framework.Core`, `Microsoft.Data.Sqlite`, and SQLite native provider packages. It does not depend on Engine, Memory, Files, EF Core, or application projects.

```text
Leva.Framework.Core
  -> .NET only

Leva.Framework.Storage
  -> Leva.Framework.Core

Leva.Framework.Storage.Sqlite
  -> Leva.Framework.Storage
  -> Leva.Framework.Core
  -> Microsoft.Data.Sqlite
```

## Project overview

The SQLite provider is a complete provider for the storage contracts. It stores all repositories in one provider-owned repository table and all journals in one provider-owned journal table. Named repositories and journals are separated by stable hashed provider names.

The provider is useful for durable local framework data when files are too loose but EF Core would be unnecessary. It should stay a small SQLite implementation of the storage contracts, not a second ORM.

## Files and classes

### Provider composition

`SqliteStorageProvider` - Creates repository and journal contracts for one SQLite database file.
`SqliteStorageDatabase` - Owns database paths, connections, serialization, timestamp formatting, and key conversion.
`SqliteStorageInitializer` - Creates the provider-owned SQLite schema when a database connection is opened.

### Repository implementation

`SqliteRepository<TId, TValue>` - Internal repository implementation that delegates keyed value operations to a named SQLite store.
`SqliteRepositoryStore<TId, TValue>` - Performs repository SQL operations, version checks, serialization, and row mapping.

### Journal implementation

`SqliteJournal<TEntry>` - Internal journal implementation that delegates append and read operations to a named SQLite store.
`SqliteJournalStore<TEntry>` - Performs journal SQL operations, sequence version assignment, serialization, and row mapping.
