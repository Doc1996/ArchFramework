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

The file provider is a complete provider for the storage contracts. It keeps each named repository in a hashed directory and stores each keyed value as one JSON file. Journals are stored as ordered JSON files named by provider-assigned sequence version.

The provider is useful for local desktop tools, simple durable framework data, samples, and hosts that want persistence without a database or EF Core.

## Files and classes

### Provider composition

`FileStorageProvider` - Creates repository and journal contracts under one root directory.
`FileStorageDatabase` - Owns root paths, key conversion, JSON serialization, provider path generation, and named store reuse.

### Repository implementation

`FileRepository<TId, TValue>` - Internal repository implementation that delegates keyed value operations to a named file store.
`FileRepositoryStore<TId, TValue>` - Reads, writes, checks, lists, and deletes repository entries in the file system.

### Journal implementation

`FileJournal<TEntry>` - Internal journal implementation that delegates append and read operations to a named file store.
`FileJournalStore<TEntry>` - Appends journal entries to ordered JSON files and reads entries in version order.

### Shared file helpers

`FileStorageRunner` - Serializes async file operations for one named store and converts unexpected exceptions to storage failures.
`FileStorageEntry<T>` - JSON storage shape used by the file provider for stored values and metadata.
