# Leva.Framework.Identity.Tests

`Leva.Framework.Identity.Tests` verifies the provider-neutral identity contracts and services from `Leva.Framework.Identity`. These tests cover principal values, sessions, authentication, authorization, audit sinks, errors, and state-facing principal access.

## Coverage

`PrincipalValueTests` - Verifies principal IDs, session IDs, default principal collections, roles, permissions, claims, authentication methods, authorization requirements, and session values.
`PrincipalErrorTests` - Verifies structured identity error codes and messages.
`AuditSinkTests` - Verifies memory and null audit sink behavior.
`PrincipalSessionServiceTests` - Verifies session creation, loading, expiration, sign-out, deletion, and audit entries.
`AuthenticationServiceTests` - Verifies authentication policy selection, session creation, sign-out, failure behavior, and audit entries.
`AuthorizationServiceTests` - Verifies authorization denial, policy evaluation order, successful authorization, failed authorization, and policy failures.
`PrincipalAccessTests` - Verifies current session resolution, principal extraction, signed-in checks, authorization checks, and source failure propagation.
`ResultAssert` - Small helper for asserting successful `Result<T>` values.
