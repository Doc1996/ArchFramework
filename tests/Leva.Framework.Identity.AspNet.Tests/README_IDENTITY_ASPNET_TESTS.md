# Leva.Framework.Identity.AspNet.Tests

`Leva.Framework.Identity.AspNet.Tests` verifies the ASP.NET Core adapter behavior for framework identity auth sessions, claims, HTTP cookie transport, sign-in, authorization bridging, endpoint contracts, Google sign-in contracts, and JWT token handling.

## Coverage

`AspNetClaimsPrincipalMapperTests` - Verifies mapping framework principals, roles, permissions, claims, and auth session ids to ASP.NET Core claims principals.
`AspNetAuthSessionTests` - Verifies reading auth session ids from cookies and optional headers, and writing/deleting auth session cookies.
`AspNetAuthSessionSourceTests` - Verifies loading the current framework auth session from the ASP.NET Core HTTP context.
`AspNetSignInServiceTests` - Verifies sign-in cookie writing and missing-session sign-out failure behavior.
`AspNetAuthorizationHandlerTests` - Verifies that ASP.NET Core authorization succeeds when framework authorization approves the requirement.
`AspNetEndpointContractTests` - Verifies login request mapping and current-principal response shaping used by the built in endpoints.
`AspNetGoogleContractTests` - Verifies Google user mapping and authorization URL/correlation cookie creation.
`AspNetJwtTests` - Verifies JWT token issuing, validation, principal/session round-tripping, and invalid-token failures.
