# Leva.Framework.Storage.Memory

`Leva.Framework.Storage.Memory` is the in-memory provider implementation of `Leva.Framework.Storage`. It stores repositories and journals in process for tests, demos, samples, and short-lived hosts.

## Purpose and dependencies

`Leva.Framework.Storage.Memory` depends on `Leva.Framework.Storage`, `Leva.Framework.Core`, and .NET base libraries. It does not depend on Engine, Files, SQLite, EF Core, databases, or application projects.

```text
Leva.Framework.Core
  -> .NET only

Leva.Framework.Storage
  -> Leva.Framework.Core

Leva.Framework.Storage.Memory
  -> Leva.Framework.Storage
  -> Leva.Framework.Core
```

## Project overview

The memory provider is a complete provider for the storage contracts. It keeps named repositories and journals inside one provider instance. Repositories store keyed values with per-record versions, while journals append ordered entries with sequence versions.

The provider is intentionally not durable. It is useful when a host needs the same storage contracts without file, database, or EF Core setup.

## Files and classes

### Provider composition

`MemoryStorageProvider` - Creates repository and journal contracts for one in-memory provider instance.
`MemoryStorageDatabase` - Owns named in-memory repository and journal stores for one provider instance.

### Repository implementation

`MemoryRepository<TId, TValue>` - Internal repository implementation that delegates keyed value operations to a named in-memory store.
`MemoryRepositoryStore<TId, TValue>` - Holds repository entries, performs version checks, and returns snapshots of stored values.

### Journal implementation

`MemoryJournal<TEntry>` - Internal journal implementation that delegates append and read operations to a named in-memory store.
`MemoryJournalStore<TEntry>` - Holds journal entries and assigns sequence versions.
