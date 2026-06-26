# Leva.Framework.Identity.Memory.Tests

`Leva.Framework.Identity.Memory.Tests` verifies the in-memory provider implementation of `Leva.Framework.Identity`. These tests cover principal storage, session storage, memory authentication, memory authorization, current-session resolution, and provider composition.

## Coverage

`MemoryPrincipalStoreTests` - Verifies adding, loading, finding, removing, and clearing principals.
`MemoryPrincipalSessionStoreTests` - Verifies saving, loading, deleting, and clearing principal sessions.
`MemoryPrincipalSessionSourceTests` - Verifies configured current-session loading and `PrincipalAccess` integration.
`MemoryAuthenticationPolicyTests` - Verifies successful authentication, invalid credentials, and session creation through `AuthenticationService`.
`MemoryAuthorizationPolicyTests` - Verifies signed-in, role, permission, and claim authorization behavior.
`ResultAssert` - Small helper for asserting successful result values.
