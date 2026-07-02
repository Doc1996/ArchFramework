# Leva.Framework.Sample.LocalAccount

Web sample for Identity.Local, Identity.AspNet, Identity.Memory, and ASP.NET Core. It creates one local account, logs in with a local secret, reads the cookie-backed principal, checks authorization, and verifies wrong-password rejection.

```text
Leva.Framework.Sample.LocalAccount
  -> Leva.Framework.Core
  -> Leva.Framework.Identity
  -> Leva.Framework.Identity.Memory
  -> Leva.Framework.Identity.Local
  -> Leva.Framework.Identity.AspNet
  -> Microsoft.AspNetCore
```

## Wiring

```text
Program.cs
  -> creates MemoryPrincipalStore and MemoryAuthSessionStore
  -> creates LocalPrincipalServices over the memory principal store
  -> creates AuthenticationService with LocalAuthenticationPolicy
  -> creates AuthorizationService with BuiltInAuthorizationPolicy
  -> creates admin principal and local password credential
  -> registers framework services in ASP.NET Core DI
  -> calls AddAspNetPrincipalServices(...)
  -> calls UseAuthentication() and UseAuthorization()
  -> calls MapAspNetIdentityEndpoints()
  -> maps /account/admin-area with RequireAuthorization("AccountManage")

AccountManage policy
  -> wraps AuthorizationRequirement.Permission("account.manage")
  -> lets ASP.NET Core authorization delegate the real permission check to Leva.Framework.Identity
```

## Files

`Program.cs` - Local identity setup, ASP.NET adapter setup, auth endpoints, and protected endpoint.
`wwwroot/` - Browser page, CSS, deterministic scenario checks, and manual current-session actions.

Configured account:

```text
name: admin
secret: password
permission: account.manage
```

## Run

```bash
dotnet run --project samples/Leva.Framework.Sample.LocalAccount
```

Expected result from **Run all checks**:

```text
PASS: initial principal is anonymous
PASS: anonymous admin access is rejected with 401
PASS: login succeeds and writes auth cookie
PASS: authenticated principal has admin role and account.manage permission
PASS: authorization policy allows admin area after login
PASS: wrong password is rejected
PASS: logout returns to anonymous principal
```

The manual buttons show the current browser session state only. They do not produce PASS/FAIL test results, because authentication state is intentionally stateful.
