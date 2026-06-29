# Leva.Framework.Identity.Memory

`Leva.Framework.Identity.Memory` is the in-memory provider implementation of `Leva.Framework.Identity`. It stores principals and sessions in process and authenticates configured principal names for tests, demos, samples, and early applications.

## Purpose and dependencies

Memory identity exists for tests, demos, samples, and short-lived local application scenarios where durable identity storage is not required. It depends on `Leva.Framework.Identity` and `Leva.Framework.Core`; it does not depend on Engine, Storage, ASP.NET Core, databases, password hashing packages, or external identity providers.

```text
Leva.Framework.Identity.Memory
  -> Leva.Framework.Identity
  -> Leva.Framework.Core
  -> .NET base libraries
```

## Project overview

The memory provider is intentionally simple. `MemoryPrincipalStore` keeps principals in memory. `MemoryPrincipalSessionStore` keeps sessions in memory. `MemoryAuthenticationPolicy` authenticates a configured name to a configured `PrincipalId` using the `memory.principal` method. It does not store passwords, secrets, reset tokens, email confirmation state, lockout state, or account lifecycle data.

Authorization is not memory-specific. `MemoryPrincipalServices` uses the shared `PrincipalAuthorizationPolicy` from `Leva.Framework.Identity` for signed-in, role, permission, and claim checks. `MemoryPrincipalSessionSource` exposes one configured current session for tests, demos, and simple hosts.

`MemoryPrincipalServices` is a small composition helper. It creates the memory stores, authentication policy, shared authorization policy, session service, authentication service, authorization service, session source, and principal access object while still leaving each individual object available for direct testing or manual wiring.

## Files and classes

### Provider composition

`MemoryPrincipalServices` - Groups the memory stores, policies, services, session source, and principal access object for quick setup.

### Principal storage

`MemoryPrincipalStore` - Stores principals in memory and resolves them by id, display name, email, or configured names.

### Session implementation

`MemoryPrincipalSessionStore` - Stores principal sessions in memory and implements `IPrincipalSessionStore`.
`MemoryPrincipalSessionSource` - Resolves the current principal session from a configured in-memory session id.

### Authentication implementation

`MemoryAuthenticationMethods` - Defines the standard `memory.principal` authentication method.
`MemoryAuthenticationPolicy` - Authenticates configured names by resolving them to existing principals.
