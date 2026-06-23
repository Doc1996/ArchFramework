# Leva.Framework.Storage

`Leva.Framework.Storage` is the provider-neutral persistence contract layer of ArchFramework. It defines repositories, journals, storage sessions, stored entries, versions, and common storage errors used by provider libraries and applications.

## Purpose and dependencies

Storage exists so applications and provider libraries can share the same persistence vocabulary without coupling the framework to a specific database, serializer, file format, or infrastructure provider. It contains contracts and small storage values, not concrete persistence behavior. `Leva.Framework.Storage` depends on `Leva.Framework.Core` so storage operations use the same `Result`, `Result<T>`, and `Error` model as the rest of the framework. Core must not depend on Storage, and Engine should not depend on Storage directly; hosts and infrastructure adapters should connect storage explicitly at the application boundary.

```text
Leva.Framework.Storage
  -> Leva.Framework.Core
  -> .NET base libraries

Applications / hosts
storage providers
  -> Leva.Framework.Storage
  -> Leva.Framework.Core

Leva.Framework.Engine
  -> Leva.Framework.Core
```

## Project overview

Storage is intentionally contracts-first. It defines what the framework expects from persistence while leaving implementation details to provider libraries. An in-memory provider can keep values directly, a file-system provider can use local JSON files, and a SQLite provider can use tables and provider-owned schema migration.

Repositories are the general durable model store. They store keyed values, return stored values with provider metadata through `StorageEntry<T>`, and use optional expected versions for optimistic concurrency. Framework snapshots do not need a separate snapshot-store contract because they can be stored through a repository with a string key.

Journals are append-only stores for durable history, audit trails, runtime records, or replayable entries. A journal appends records and reads them in provider-assigned version order, optionally after a known version and with a maximum count.

Storage sessions are provider-owned operation boundaries. They allow providers to group related repository and journal work behind one commit or rollback decision without exposing database transactions, connections, or provider-specific objects. `CommitAsync` makes pending work durable, `RollbackAsync` abandons pending work, and asynchronous disposal performs cleanup.

## Files and classes

### Repository contracts

`IRepository<TId, TModel>` - Stores keyed models without exposing provider-specific persistence details. It checks existence, loads one model, loads all models, saves with an optional expected version, and deletes with an optional expected version.

### Journal contracts

`IJournal<TEntry>` - Stores append-only entries for durable history, audit trails, runtime records, or replayable data. It appends one entry and reads entries in version order, optionally after a known version and with a maximum count.

### Session contracts

`IStorageSession` - Represents a provider-owned operation boundary for related storage work. It exposes commit and rollback operations while using asynchronous disposal for cleanup.
`IStorageSessionFactory` - Opens provider-owned storage sessions without exposing transaction, connection, or provider-specific objects.

### Shared storage values

`StorageEntry<T>` - Represents one stored value together with provider-owned metadata such as version, creation time, update time, and optional properties.
`StorageVersion` - Represents a positive provider-assigned storage revision or journal sequence number.
`StorageErrors` - Creates common structured storage errors for missing records, version conflicts, unavailable providers, and failed operations.
