# Leva.Framework.Storage.Files

`Leva.Framework.Storage.Files` provides local file-based implementations of the provider-neutral contracts from `Leva.Framework.Storage`.

## Purpose and dependencies

Files exists for small local applications, development tools, readable storage, and simple durable snapshots without a database server. It depends on `Leva.Framework.Storage` and `Leva.Framework.Core`; it uses .NET file APIs and JSON serialization internally.

```text
Leva.Framework.Storage.Files
  -> Leva.Framework.Storage
  -> Leva.Framework.Core
  -> .NET base libraries
```

## Project overview

The provider stores repository records and journal entries under one root directory. Repositories store one JSON file per model. Journals store one JSON file per appended entry using ordered version file names.

Storage sessions work on an isolated temporary directory copy. Commit replaces the provider root with the session copy, while rollback or disposal abandons the copy.

The public repository and journal classes stay thin. They delegate storage behavior to internal stores, while `FileStorageDatabase` owns paths, serialization, key conversion, and directory copying.

## Files and classes

### Provider entry point

`FileStorageProvider` - Creates repositories, journals, and storage sessions under one root directory.
`FileStorageDatabase` - Owns file paths, serialization, key conversion, and directory copying for one provider instance.
`FileStorageEntry<T>` - Represents one stored value in the file provider JSON format.

### Repository implementation

`FileRepository<TId, TModel>` - Stores keyed models as JSON files under a named repository folder.
`FileRepositoryStore<TId, TModel>` - Holds file operations for one named repository folder.

### Journal implementation

`FileJournal<TEntry>` - Stores append-only journal entries as ordered JSON files under a named journal folder.
`FileJournalStore<TEntry>` - Holds file operations for one named append-only journal folder.

### Session implementation

`FileStorageSession` - Owns an isolated file storage directory copy until commit or rollback.
