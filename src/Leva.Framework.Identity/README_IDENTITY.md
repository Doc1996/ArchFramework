# Leva.Framework.Identity

`Leva.Framework.Identity` is the provider-neutral identity contract layer of ArchFramework. It defines identities, sessions, authentication, authorization, audit records, and state-facing identity access used by provider libraries and applications.

## Purpose and dependencies

Identity exists so applications and provider libraries can share the same identity vocabulary without coupling the framework to a specific login provider, account store, web framework, token format, password system, or external identity service. It contains contracts, small identity values, and framework-owned orchestration services, not concrete account persistence, ASP.NET integration, Google sign-in, JWT handling, or application-specific profile data.

`Leva.Framework.Identity` depends on `Leva.Framework.Core` so identity operations use the same `Result`, `Result<T>`, and `Error` model as the rest of the framework. Core must not depend on Identity, and Engine should not depend on Identity directly; hosts and application access objects should connect identity explicitly at the application boundary.

```text
Leva.Framework.Identity
  -> Leva.Framework.Core
  -> .NET base libraries

Applications / hosts
identity providers
  -> Leva.Framework.Identity
  -> Leva.Framework.Core

Leva.Framework.Engine
  -> Leva.Framework.Core
```

## Project overview

Identity is intentionally provider-neutral. It defines who an identity is, how an identity session is represented, how authentication policies establish a session, how authorization policies check requirements, and how security-relevant actions can be audited.

Authentication and authorization are separate concepts. Authentication establishes identity through an `AuthenticationMethod` and `AuthenticationPolicy`. Authorization checks whether an established identity satisfies an `AuthorizationRequirement` through an `AuthorizationPolicy`. `AuthenticationService` and `AuthorizationService` are framework-owned orchestration classes that coordinate policies, sessions, results, and audit output.

Sessions represent active signed-in identity state. `IdentitySessionService` creates, loads, signs out, expires, and deletes sessions using `IdentitySessionStore` and `IdentitySessionPolicy`. Provider libraries decide where identities and sessions are stored. A memory provider can keep them in process, a local provider can use application storage and password hashing, an ASP.NET Core provider can resolve the current session from web context, and later Google or JWT providers can plug in without changing this library.

Identity keeps profile data minimal. `Identity` contains identity ID, display name, email, roles, permissions, and claims. Application-specific profile fields such as phone number, address, age, avatar, locale, or preferences should live in application models or provider-specific claims, not in the framework identity model.

Audit records are separate from runtime logs. `AuditEntry` records security-relevant identity actions such as authentication, sign-out, session changes, and authorization checks. `IAuditSink` receives those entries without coupling identity audit history to the engine runtime log. Applications can later adapt audit entries to logs, journals, files, databases, or external systems.

## Files and classes

### Identity model

`IdentityId` - Identifies one identity independently of provider-specific account IDs.
`Identity` - Represents a signed-in identity with display name, optional email, roles, permissions, and claims.
`IdentityClaim` - Represents one typed fact about an identity.
`IdentityRole` - Represents one role assigned to an identity.
`IdentityPermission` - Represents one precise capability assigned to an identity.
`IdentityStore` - Loads identities from provider-specific stores or account sources.

### Sessions

`IdentitySessionId` - Identifies one active or historical identity session.
`IdentitySession` - Represents an established identity session with identity, status, creation time, optional expiration time, and optional sign-out time.
`IdentitySessionStatus` - Describes identity session lifecycle state.
`IdentitySessionPolicy` - Creates session IDs, calculates expiration times, and checks whether sessions are expired.
`IdentitySessionStore` - Stores and loads identity sessions without exposing provider-specific storage details.
`IdentitySessionService` - Creates, loads, signs out, expires, and deletes identity sessions.
`IdentitySessionResolver` - Resolves the current identity session from the host or provider-specific execution context.

### Authentication

`AuthenticationMethod` - Identifies one provider-defined way to authenticate without predefined framework methods.
`AuthenticationRequest` - Represents an authentication attempt with method, name, secret, token, and optional provider properties.
`AuthenticationResult` - Represents the result of an authentication attempt, including identity, session, success flag, and optional reason.
`AuthenticationPolicy` - Authenticates requests for one or more provider-defined methods.
`AuthenticationService` - Runs authentication policies, creates sessions, signs sessions out, and writes audit entries.

### Authorization

`AuthorizationRequirement` - Represents one authorization requirement such as a role, permission, claim, or provider-defined rule.
`AuthorizationRequest` - Represents an authorization check for an identity, optional session, and requirement.
`AuthorizationResult` - Represents the result of an authorization check with success flag, requirement, and optional reason.
`AuthorizationPolicy` - Evaluates whether an authorization request satisfies one provider-defined policy.
`AuthorizationService` - Runs authorization policies and writes audit entries for authorization results.

### Audit

`AuditAction` - Describes the security-relevant action recorded by an audit entry.
`AuditEntry` - Represents one identity audit record with action, time, optional identity, optional session, method, requirement, and reason.
`IAuditSink` - Receives identity audit entries without coupling audit history to a concrete log or persistence provider.
`MemoryAuditSink` - Thread-safe in-memory audit sink for diagnostics and tests.
`NullAuditSink` - Audit sink implementation that intentionally ignores audit entries.

### State access and errors

`IdentityAccess` - State-facing identity capability that resolves the current session and performs authorization checks.
`IdentityErrors` - Creates common structured identity errors for missing resources, conflicts, invalid data, unavailable providers, failed operations, unauthorized requests, and forbidden actions.