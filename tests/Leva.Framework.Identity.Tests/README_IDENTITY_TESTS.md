# Leva.Framework.Identity.Tests

`Leva.Framework.Identity.Tests` verifies the provider-neutral identity contracts, services, access wrapper, audit sinks, errors, and value objects.

## Purpose and dependencies

Identity tests exist to document the expected behavior of `Leva.Framework.Identity` without relying on concrete external authentication providers. The tests depend on Core, Identity, and Fakes.

```text
Leva.Framework.Identity.Tests
  -> Leva.Framework.Fakes
  -> Leva.Framework.Identity
  -> Leva.Framework.Core
```

## Project overview

The tests cover identity value objects, structured errors, audit sinks, session lifecycle behavior, authentication service policy selection, authorization service policy evaluation, and state-facing `IdentityAccess` behavior.

## Files and classes

### Test helpers

`ResultAssert` - Small helper for asserting successful `Result<T>` values.

### Identity values and errors

`IdentityValueTests` - Verifies identity IDs, default collections, roles, permissions, claims, and authorization requirement formatting.
`IdentityErrorTests` - Verifies structured identity error codes and messages.

### Audit and sessions

`AuditSinkTests` - Verifies memory and null audit sinks.
`IdentitySessionServiceTests` - Verifies session creation, loading, expiration, sign-out, deletion, and audit entries.

### Authentication and authorization

`AuthenticationServiceTests` - Verifies authentication policy selection, session creation, sign-out, failure behavior, and audit entries.
`AuthorizationServiceTests` - Verifies authorization denial, policy evaluation order, successful authorization, failed authorization, and policy failures.

### State access

`IdentityAccessTests` - Verifies current session resolution, identity extraction, signed-in checks, authorization checks, and source failure propagation.
