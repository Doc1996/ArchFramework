# Leva.Framework.Storage.Memory.Tests

`Leva.Framework.Storage.Memory.Tests` verifies the in-memory provider implementation of `Leva.Framework.Storage`. These tests cover memory repositories, journals, sessions, provider ownership, and deterministic commit/rollback behavior.

## Coverage

`MemoryRepositoryTests` - Verifies save, load, load all, exists, delete, updates, and optimistic version conflicts.
`MemoryJournalTests` - Verifies append order, sequence versions, filtered reads, limited reads, and invalid limits.
`MemoryStorageSessionTests` - Verifies commit, rollback, completed session behavior, and provider ownership checks.
