# Leva.Framework.Storage.Files.Tests

`Leva.Framework.Storage.Files.Tests` verifies the local file provider implementation of `Leva.Framework.Storage`. These tests cover file repositories, file journals, persisted reads, deletion, and versioning behavior.

## Coverage

`FileRepositoryTests` - Verifies save, load, load all, exists, delete, persisted reads, not-found, and version conflict behavior.
`FileJournalTests` - Verifies append, read, persisted reads, version ordering, after-version filtering, and limits.
`TestStorageDirectory` - Internal test helper that provides isolated temporary directories for file provider tests.
