# Leva.Framework.Storage.Files.Tests

`Leva.Framework.Storage.Files.Tests` verifies the local file provider implementation of the storage contracts. These tests cover file repositories, file journals, persisted reads, and session commit/rollback behavior.

## Coverage

`FileRepositoryTests` - Verifies save, load, persisted load, load all, exists, delete, and optimistic version conflicts.
`FileJournalTests` - Verifies append order, sequence versions, persisted reads, filtered reads, limited reads, and invalid limits.
`FileStorageSessionTests` - Verifies commit, rollback, completed session behavior, and provider ownership checks.
