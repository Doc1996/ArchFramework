# Leva.Framework.Identity.Local

`Leva.Framework.Identity.Local` is the local/offline provider implementation of `Leva.Framework.Identity`. It authenticates principals with local names and protected secrets without depending on ASP.NET Core or external identity providers.

## Purpose and dependencies

Local identity exists for desktop tools, kiosk applications, robotics and medical-device software, internal utilities, demos, and offline applications that need local sign-in without a web authentication stack. It depends on `Leva.Framework.Identity`, `Leva.Framework.Core`, and .NET cryptography libraries. It does not depend on Engine, Storage, ASP.NET Core, databases, UI frameworks, email systems, or external identity providers.

```text
Leva.Framework.Identity.Local
  -> Leva.Framework.Identity
  -> Leva.Framework.Core
  -> .NET cryptography libraries
```

## Project overview

The local provider keeps account lifecycle concerns small. It supports local credentials, protected secrets, credential storage, credential changes, enable/disable, deletion, and local secret authentication through the `local.secret` method. It does not include registration screens, email confirmation, password reset flows, lockout, two-factor authentication, profile editing, invitations, or web cookies.

`LocalCredentialService` creates and updates credentials. `LocalAuthenticationPolicy` verifies a submitted name and secret, resolves the linked principal, and returns an authentication result. `LocalPrincipalServices` wires a credential store, secret protector, credential service, and authentication policy for local/offline hosts.

## Files and classes

### Provider composition

`LocalPrincipalServices` - Groups the local credential store, secret protector, credential service, and authentication policy for quick setup.

### Credentials

`LocalCredential` - Connects a local login name, principal id, protected secret, enabled flag, and timestamps.
`LocalCredentialSecret` - Stores protected secret value, salt, iteration count, and algorithm name.
`ILocalCredentialStore` - Stores and loads local credentials by name.
`MemoryLocalCredentialStore` - In-memory local credential store for tests, demos, and simple local hosts.
`LocalCredentialService` - Creates, changes, enables, disables, and deletes local credentials.

### Secrets

`LocalSecretProtector` - PBKDF2-based local secret protector that creates and verifies local credential secrets.

### Authentication

`LocalAuthenticationMethods` - Defines the standard `local.secret` authentication method.
`LocalAuthenticationPolicy` - Authenticates local name and secret credentials and resolves the linked principal.
