# Leva.Framework.Identity.Memory.Tests

`Leva.Framework.Identity.Memory.Tests` verifies the in-memory provider implementation of `Leva.Framework.Identity`. These tests cover principal storage, auth session storage, memory authentication, current-auth-session resolution, and provider composition.

## Coverage

`MemoryPrincipalStoreTests` - Verifies adding, loading, finding, removing, and clearing principals.
`MemoryAuthSessionStoreTests` - Verifies saving, loading, deleting, and clearing auth sessions.
`MemoryAuthSessionSourceTests` - Verifies configured current-auth-session loading and `PrincipalAccess` integration.
`MemoryAuthenticationPolicyTests` - Verifies successful authentication, invalid credentials, and auth session creation through `AuthenticationService`.
