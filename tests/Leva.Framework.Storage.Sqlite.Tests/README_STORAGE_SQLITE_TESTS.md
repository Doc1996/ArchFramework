# Leva.Framework.Storage.Sqlite.Tests

`Leva.Framework.Storage.Sqlite.Tests` verifies the SQLite provider implementation of the storage contracts. These tests cover SQLite repositories, SQLite journals, persisted reads, and transaction-backed session commit/rollback behavior.

## Coverage

`SqliteRepositoryTests` - Verifies save, load, persisted load, load all, exists, delete, and optimistic version conflicts.
`SqliteJournalTests` - Verifies append order, sequence versions, persisted reads, filtered reads, limited reads, and invalid limits.
`SqliteStorageSessionTests` - Verifies commit, rollback, dispose rollback, completed session behavior, and provider ownership checks.
