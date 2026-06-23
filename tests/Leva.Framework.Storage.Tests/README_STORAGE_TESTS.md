# Leva.Framework.Storage.Tests

`Leva.Framework.Storage.Tests` verifies the stable contracts and value types from `Leva.Framework.Storage`. These tests focus on storage entries, versions, errors, repository contracts, journal contracts, and session contracts rather than provider-specific behavior.

## Coverage

`StorageEntryTests` - Verifies stored value metadata, versions, timestamps, and optional properties.
`StorageVersionTests` - Verifies positive storage versions, next-version behavior, equality, and invalid version guards.
`StorageErrorsTests` - Verifies common storage error codes and messages.
`RepositoryContractTests` - Verifies that repository contracts can be implemented and used with keyed models.
`JournalContractTests` - Verifies that journal contracts can be implemented and used with append-only entries.
`StorageSessionContractTests` - Verifies that session and session factory contracts can be implemented and used together.
