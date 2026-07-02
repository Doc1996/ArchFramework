# Leva.Framework.Sample.LocalIdentity

Web sample for Identity.Local, Identity.AspNet, Identity.Memory, and ASP.NET Core. It creates local accounts, logs in with local secrets, reads the cookie-backed principal, checks authorization, and verifies wrong-password rejection.

```text
Leva.Framework.Sample.LocalIdentity
  -> Leva.Framework.Core
  -> Leva.Framework.Identity
  -> Leva.Framework.Identity.Memory
  -> Leva.Framework.Identity.Local
  -> Leva.Framework.Identity.AspNet
  -> Microsoft.AspNetCore
```

## Setup shown by the sample

```text
Program.cs
  -> creates MemoryPrincipalStore and MemoryAuthSessionStore
  -> creates LocalPrincipalServices over the memory principal store
  -> creates AuthenticationService with LocalAuthenticationPolicy
  -> creates AuthorizationService with BuiltInAuthorizationPolicy
  -> creates local principals and password credentials
  -> registers framework services in ASP.NET Core DI
  -> calls AddAspNetPrincipalServices(...)
  -> calls UseAuthentication() and UseAuthorization()
  -> calls MapAspNetIdentityEndpoints()
  -> maps /account/admin-area with RequireAuthorization("AccountManage")

AccountManage policy
  -> wraps AuthorizationRequirement.Permission("account.manage")
  -> lets ASP.NET Core authorization delegate the real permission check to Leva.Framework.Identity
```

## Accounts

```text
admin / password
  -> role: admin
  -> permission: account.manage

operator / password
  -> role: operator
  -> no account.manage permission
```

The accounts are listed here instead of on the page so the browser UI stays focused on behavior.

## Behavior shown

```text
anonymous principal       -> not authenticated
anonymous admin access    -> rejected with 401
operator login            -> succeeds, but admin area is rejected
admin login               -> succeeds and writes auth-session cookie
admin area after login    -> succeeds
wrong password            -> rejected with 401
logout                    -> returns to anonymous principal
```

The manual buttons show the current browser session state. They do not produce PASS/FAIL results because authentication state is intentionally stateful.

## Run

```bash
dotnet run --project samples/Leva.Framework.Sample.LocalIdentity
```

## Expected result

```text
PASS: initial principal is anonymous
PASS: anonymous admin access is rejected
PASS: operator login succeeds
PASS: operator principal is authenticated without account.manage
PASS: operator cannot access account.manage endpoint
PASS: admin login succeeds
PASS: admin principal has admin role and account.manage permission
PASS: admin principal can access account.manage endpoint
PASS: wrong password is rejected
PASS: logout returns to anonymous principal
```
