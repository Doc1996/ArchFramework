# Leva.Framework.Storage.Memory.Tests

`Leva.Framework.Storage.Memory.Tests` verifies the in-memory provider implementation of `Leva.Framework.Storage`. These tests cover memory repositories, journals, versioning, deletion, and provider instance behavior.

## Coverage

`MemoryRepositoryTests` - Verifies save, load, load all, exists, delete, not-found, and version conflict behavior.
`MemoryJournalTests` - Verifies append, read, version ordering, after-version filtering, and limits.
