# Leva.Framework.Identity.Tests

`Leva.Framework.Identity.Tests` verifies the provider-neutral principal contracts and services from `Leva.Framework.Identity`. These tests cover principal values, auth sessions, authentication, authorization, audit sinks, errors, and state-facing principal access.

## Coverage

`PrincipalValueTests` - Verifies principal IDs, auth session IDs, default principal collections, roles, permissions, claims, authentication methods, authorization requirements, and auth session values.
`PrincipalErrorTests` - Verifies structured identity error codes and messages.
`AuditSinkTests` - Verifies memory and null audit sink behavior.
`AuthSessionServiceTests` - Verifies auth session creation, loading, expiration, sign-out, deletion, and audit entries.
`AuthenticationServiceTests` - Verifies authentication policy selection, auth session creation, sign-out, failure behavior, and audit entries.
`AuthorizationServiceTests` - Verifies authorization denial, policy evaluation order, successful authorization, failed authorization, and policy failures.
`BuiltInAuthorizationPolicyTests` - Verifies signed-in, role, permission, and claim authorization behavior.
`PrincipalAccessTests` - Verifies current auth session resolution, principal extraction, signed-in checks, authorization checks, and source failure propagation.
