# Leva.Framework.Storage.Files

`Leva.Framework.Storage.Files` is the local file provider implementation of `Leva.Framework.Storage`. It stores repositories and journals as JSON files under one root directory.

## Purpose and dependencies

`Leva.Framework.Storage.Files` depends on `Leva.Framework.Storage`, `Leva.Framework.Core`, and .NET file/JSON libraries. It does not depend on Engine, Memory, SQLite, EF Core, databases, or application projects.

```text
Leva.Framework.Core
  -> .NET only

Leva.Framework.Storage
  -> Leva.Framework.Core

Leva.Framework.Storage.Files
  -> Leva.Framework.Storage
  -> Leva.Framework.Core
```

## Project overview

The file provider is a complete provider for the storage contracts. It keeps each named repository in a hashed directory and stores each keyed model as one JSON file. Journals are stored as ordered JSON files named by provider-assigned sequence version.

The provider is useful for local desktop tools, simple durable framework data, samples, and hosts that want persistence without a database or EF Core.

## Files and classes

### Provider composition

`FileStorageProvider` - Creates repositories and journals under one root directory.
`FileStorageDatabase` - Owns root paths, key conversion, JSON serialization, and provider path generation.

### Repository implementation

`FileRepository<TId, TModel>` - Public repository implementation that delegates keyed model operations to a named file store.
`FileRepositoryStore<TId, TModel>` - Reads, writes, checks, lists, and deletes repository entries in the file system.

### Journal implementation

`FileJournal<TEntry>` - Public journal implementation that delegates append and read operations to a named file store.
`FileJournalStore<TEntry>` - Appends journal entries to ordered JSON files and reads entries in version order.

### File values

`FileStorageEntry<T>` - JSON storage shape used by the file provider for stored values and metadata.
