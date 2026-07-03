# Leva.Framework.Contract.Tests

`Leva.Framework.Contract.Tests` verifies shared behavior across framework providers that implement the same abstractions. These tests cover storage repositories, storage journals, notification storage and gateways, auth sessions, memory principals, and local credentials.

## Coverage

`RepositoryContractTests` - Verifies save, load, update, version conflict rejection, existence checks, load-all behavior, deletion, and repository isolation across memory, file, and SQLite storage providers.
`JournalContractTests` - Verifies append behavior, monotonic versions, ordered reads, after-version reads, limited reads, and journal isolation across memory, file, and SQLite storage providers.
`NotificationStoreContractTests` - Verifies notification save, load, replacement by notification ID, load-all behavior, deletion, and idempotent deletion.
`NotificationGatewayContractTests` - Verifies that gateway delivery preserves the notification payload sent by the notification service.
`AuthSessionStoreContractTests` - Verifies auth session save, load, replacement by session ID, deletion, and idempotent deletion.
`MemoryPrincipalStoreContractTests` - Verifies principal loading by ID, explicit principal names, display name, email, and removal of all stored names for a principal.
`MemoryLocalCredentialStoreContractTests` - Verifies local credential save, case-insensitive username lookup, replacement, deletion, and idempotent deletion.
