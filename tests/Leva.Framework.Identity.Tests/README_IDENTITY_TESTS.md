# Leva.Framework.Identity.Tests

`Leva.Framework.Identity.Tests` verifies the provider-neutral identity contracts and services from `Leva.Framework.Identity`. These tests cover identity values, sessions, authentication, authorization, audit sinks, errors, and state-facing identity access.

## Coverage

`IdentityValueTests` - Verifies identity IDs, session IDs, default identity collections, roles, permissions, claims, authentication methods, authorization requirements, and session values.
`IdentityErrorTests` - Verifies structured identity error codes and messages.
`AuditSinkTests` - Verifies memory and null audit sink behavior.
`IdentitySessionServiceTests` - Verifies session creation, loading, expiration, sign-out, deletion, and audit entries.
`AuthenticationServiceTests` - Verifies authentication policy selection, session creation, sign-out, failure behavior, and audit entries.
`AuthorizationServiceTests` - Verifies authorization denial, policy evaluation order, successful authorization, failed authorization, and policy failures.
`IdentityAccessTests` - Verifies current session resolution, identity extraction, signed-in checks, authorization checks, and source failure propagation.
`ResultAssert` - Small helper for asserting successful `Result<T>` values.
