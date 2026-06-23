# Leva.Framework.Storage.Memory

`Leva.Framework.Storage.Memory` provides in-process implementations of the provider-neutral contracts from `Leva.Framework.Storage`.

## Purpose and dependencies

Memory exists for tests, demos, samples, and short-lived local runtime scenarios where durable persistence is not required. It depends on `Leva.Framework.Storage` and `Leva.Framework.Core`; it does not depend on file systems, databases, serializers, or external provider packages.

```text
Leva.Framework.Storage.Memory
  -> Leva.Framework.Storage
  -> Leva.Framework.Core
  -> .NET base libraries
```

## Project overview

The provider keeps repositories and journals inside one `MemoryStorageProvider` instance. Values remain available while that provider instance is alive and are lost when it is discarded.

Repositories store keyed models with incrementing per-record versions. Journals append ordered entries with incrementing sequence versions. Storage sessions work on an isolated copy of the provider stores; commit replaces the root stores with the session copy, while rollback or disposal abandons the copy.

The public repository and journal classes stay thin. They delegate storage behavior to internal stores, while `MemoryStorageDatabase` owns named store buffers and `MemoryStorageSession` owns the temporary provider copy.

## Files and classes

### Provider entry point

`MemoryStorageProvider` - Creates repositories, journals, and storage sessions for one provider instance.
`MemoryStorageDatabase` - Owns named repository and journal store buffers for one provider instance.

### Repository implementation

`MemoryRepository<TId, TModel>` - Implements keyed repository storage over an in-memory repository store.
`MemoryRepositoryStore<TId, TModel>` - Holds repository entries, performs version checks, and can be cloned for sessions.

### Journal implementation

`MemoryJournal<TEntry>` - Implements append-only journal storage over an in-memory journal store.
`MemoryJournalStore<TEntry>` - Holds journal entries, assigns sequence versions, and can be cloned for sessions.

### Session implementation

`MemoryStorageSession` - Holds an isolated provider copy until commit, rollback, or disposal.
`IMemoryStoreBuffer` - Internal copyable buffer contract used by storage sessions.
