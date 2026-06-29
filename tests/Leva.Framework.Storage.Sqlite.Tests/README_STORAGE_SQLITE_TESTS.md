# Leva.Framework.Storage.Sqlite.Tests

`Leva.Framework.Storage.Sqlite.Tests` verifies the SQLite provider implementation of `Leva.Framework.Storage`. These tests cover SQLite repositories, SQLite journals, persisted reads, deletion, and versioning behavior.

## Coverage

`SqliteRepositoryTests` - Verifies save, load, load all, exists, delete, persisted reads, not-found, and version conflict behavior.
`SqliteJournalTests` - Verifies append, read, persisted reads, version ordering, after-version filtering, and limits.
`TestSqliteDatabase` - Provides isolated temporary database files for SQLite provider tests.
