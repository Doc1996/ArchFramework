# Leva.Framework.Identity.Local.Tests

`Leva.Framework.Identity.Local.Tests` verifies the local/offline provider implementation of `Leva.Framework.Identity`. These tests cover local credentials, protected secrets, local authentication, in-memory credential storage, and local service composition.

## Coverage

`LocalAuthenticationPolicyTests` - Verifies valid local secret authentication, invalid secrets, disabled credentials, and auth session creation through authentication services.
`LocalCredentialServiceTests` - Verifies credential creation, secret changes, enable/disable behavior, and deletion.
`LocalPrincipalServicesTests` - Verifies local provider composition and authentication wiring.
`MemoryLocalCredentialStoreTests` - Verifies saving, loading, deleting, and clearing local credentials in memory.
`LocalSecretProtectorTests` - Verifies protected secret creation, verification, and invalid-secret rejection.
