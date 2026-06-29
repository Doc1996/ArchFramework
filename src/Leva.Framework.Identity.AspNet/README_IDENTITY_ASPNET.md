# Leva.Framework.Identity.AspNet

`Leva.Framework.Identity.AspNet` is the ASP.NET Core adapter for `Leva.Framework.Identity`. It connects framework principals and auth sessions to ASP.NET Core authentication, authorization, HTTP cookies, built in endpoints, Google sign-in, and JWT bearer tokens.

## Purpose and dependencies

ASP.NET Core identity exists so web hosts can use the provider-neutral Identity library through normal ASP.NET Core middleware and endpoint patterns. It does not implement a new identity model, database store, account registration system, profile management, or application-specific authorization rules.

`Leva.Framework.Identity.AspNet` depends on `Leva.Framework.Identity`, `Leva.Framework.Core`, and the ASP.NET Core shared framework. Identity remains provider-neutral and must not depend on ASP.NET Core. Google and JWT support live in this library because, for this framework, they are web-host authentication mechanisms rather than desktop/local identity providers.

```text
Leva.Framework.Identity.AspNet
  -> Leva.Framework.Identity
  -> Leva.Framework.Core
  -> Microsoft.AspNetCore.App

Leva.Framework.Identity
  -> Leva.Framework.Core
  -> .NET base libraries
```

## Project overview

The ASP.NET Core provider is a host adapter. It reads an `AuthSessionId` from an HTTP cookie or optional request header, loads the framework `AuthSession`, maps the session principal to an ASP.NET Core `ClaimsPrincipal`, and returns a normal ASP.NET Core authentication ticket. That allows applications to use standard ASP.NET Core features such as `UseAuthentication`, `UseAuthorization`, `[Authorize]`, and `RequireAuthorization()` without moving web concepts into the core Identity library.

Sign-in and sign-out stay framework-owned. `AspNetSignInService` calls `AuthenticationService`, writes the created auth session id to the HTTP response, and deletes it on sign-out. The built in endpoints are optional convenience endpoints for simple web hosts and samples. Applications can skip them and call `AspNetSignInService` directly from controllers, minimal APIs, pages, or Blazor endpoints.

Authorization is also delegated back to framework identity. `AspNetAuthorizationRequirement` wraps an `AuthorizationRequirement`, and `AspNetAuthorizationHandler` resolves the current auth session and calls `AuthorizationService`. Application-specific ASP.NET Core policies can therefore use the same role, permission, claim, and custom authorization model as non-web hosts.

Google support implements a compact OAuth sign-in flow for ASP.NET Core hosts. It creates a Google authorization redirect, validates the callback state, exchanges the authorization code for an access token, reads Google user information, maps it to a framework principal, and signs the user into a normal framework auth session.

JWT support issues and validates compact HMAC-signed bearer tokens for framework auth sessions. It can issue tokens from any configured framework authentication method and can authenticate incoming `Authorization: Bearer` requests through an ASP.NET Core authentication handler.

## Files and classes

### Provider composition

`AspNetPrincipalServices` - Registers ASP.NET Core authentication, authorization, HTTP session adapters, current auth session source, claim mapping, sign-in services, Google services, and JWT services.
`AspNetIdentityOptions` - Configures authentication scheme name, auth session cookie/header names, optional header support, and cookie behavior.

### Authentication implementation

`AspNetAuthenticationOptions` - ASP.NET Core authentication scheme options for framework auth sessions.
`AspNetAuthenticationHandler` - ASP.NET Core authentication handler that authenticates requests from framework auth sessions.
`AspNetAuthSessionSource` - Resolves the current auth session from the ASP.NET Core HTTP context.
`AspNetAuthSessionReader` - Reads auth session ids from ASP.NET Core request cookies or optional headers.
`AspNetAuthSessionWriter` - Writes and deletes auth session cookies on ASP.NET Core responses.
`AspNetClaimsPrincipalMapper` - Maps framework auth sessions and principals to ASP.NET Core claims principals.
`AspNetSignInService` - Signs principals in and out through framework authentication services and ASP.NET Core auth session cookies.

### Authorization implementation

`AspNetAuthorizationRequirement` - Wraps one framework `AuthorizationRequirement` for ASP.NET Core policy registration.
`AspNetAuthorizationHandler` - Delegates ASP.NET Core authorization checks to framework `AuthorizationService`.

### Built in endpoints

`AspNetIdentityEndpoints` - Maps built in `/identity/login`, `/identity/logout`, and `/identity/me` endpoints.
`AspNetLoginRequest` - Request body used by the built in login and token endpoints.
`AspNetLoginResponse` - Response body returned by the built in login endpoint.
`AspNetCurrentPrincipalResponse` - Response body returned by the built in current-principal endpoint.

### Google sign-in

`AspNetGoogleDefaults` - Holds the built in Google authentication method and endpoint defaults.
`AspNetGoogleOptions` - Configures Google client settings, callback path, scopes, correlation cookies, and return URL behavior.
`AspNetGoogleUserInfo` - Represents Google user information used to create framework principals.
`AspNetGooglePrincipalMapper` - Maps Google user information to a framework `Principal`.
`AspNetGoogleAuthenticationPolicy` - Authenticates Google access tokens and creates framework authentication results.
`AspNetGoogleSignInService` - Handles Google authorization redirects, callback validation, token exchange, and framework sign-in.
`AspNetGoogleEndpoints` - Maps built in `/identity/google/login` and `/identity/google/callback` endpoints.

### JWT bearer tokens

`AspNetJwtDefaults` - Holds the built in JWT authentication method, scheme, and claim defaults.
`AspNetJwtOptions` - Configures issuer, audience, signing key, token lifetime, clock skew, and scheme name.
`AspNetJwtClaimsMapper` - Maps framework auth sessions and principals to JWT payloads and back.
`AspNetJwtTokenService` - Issues and validates built in HMAC-signed JWT access tokens.
`AspNetJwtAuthenticationPolicy` - Authenticates JWT tokens through the framework authentication service.
`AspNetJwtAuthenticationOptions` - ASP.NET Core authentication scheme options for built in JWT bearer tokens.
`AspNetJwtAuthenticationHandler` - ASP.NET Core authentication handler that authenticates `Authorization: Bearer` requests.
`AspNetJwtEndpoints` - Maps the built in `/identity/jwt/login` endpoint for token issuing.
`AspNetJwtTokenResponse` - Response body returned by the built in JWT login endpoint.
