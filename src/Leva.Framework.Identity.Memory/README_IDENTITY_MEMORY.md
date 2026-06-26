# Leva.Framework.Identity.Memory

`Leva.Framework.Identity.Memory` provides in-process implementations of the provider-neutral contracts from `Leva.Framework.Identity`.

## Purpose and dependencies

Memory exists for tests, demos, samples, and short-lived local application scenarios where durable identity storage is not required. It depends on `Leva.Framework.Identity` and `Leva.Framework.Core`; it does not depend on Engine, Storage, ASP.NET Core, databases, password hashing packages, or external identity providers.

```text
Leva.Framework.Identity.Memory
  -> Leva.Framework.Identity
  -> Leva.Framework.Core
  -> .NET base libraries
```

## Project overview

The provider keeps principals, credentials, and sessions inside one in-process composition. Principals can be added to `MemoryPrincipalStore`, credentials can be added to `MemoryAuthenticationPolicy`, and sessions can be saved in `MemoryPrincipalSessionStore`. Values remain available while the memory objects are alive and are lost when they are discarded.

Authentication uses the `memory.secret` method and maps a configured login name and secret to a principal. Authorization evaluates signed-in, role, permission, and claim requirements from the principal model. `MemoryPrincipalSessionSource` exposes one configured current session for tests, demos, and simple local hosts.

`MemoryPrincipalServices` is a small composition helper. It creates the memory stores, policies, services, session source, and principal access object while still leaving each individual object available for direct testing or manual wiring.

## Files and classes

### Provider composition

`MemoryPrincipalServices` - Groups the memory stores, policies, services, session source, and principal access object for quick setup.

### Principal storage

`MemoryPrincipalStore` - Stores principals in memory and resolves them by id, display name, email, or configured names.
`MemoryPrincipalCredential` - Connects a login name and secret to a principal id for memory authentication.

### Session implementation

`MemoryPrincipalSessionStore` - Stores principal sessions in memory and implements `IPrincipalSessionStore`.
`MemoryPrincipalSessionSource` - Resolves the current principal session from a configured in-memory session id.

### Authentication implementation

`MemoryAuthenticationMethods` - Defines the standard `memory.secret` authentication method.
`MemoryAuthenticationPolicy` - Authenticates principals by name and secret using configured memory credentials.

### Authorization implementation

`MemoryAuthorizationPolicy` - Evaluates signed-in, role, permission, and claim requirements from the principal model.
