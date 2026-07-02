using System.Security.Claims;
using Leva.Framework.Core;
using Leva.Framework.Identity;
using Leva.Framework.Identity.AspNet;
using Leva.Framework.Identity.Local;
using Leva.Framework.Identity.Memory;
using Leva.Framework.Sample.WebIdentity;
using FrameworkAuthenticationService = Leva.Framework.Identity.AuthenticationService;
using FrameworkAuthorizationService = Leva.Framework.Identity.AuthorizationService;
using FrameworkSystemClock = Leva.Framework.Core.SystemClock;

var builder = WebApplication.CreateBuilder(args);

var principalStore = new MemoryPrincipalStore();
var sessionStore = new MemoryAuthSessionStore();
var sessionService = new AuthSessionService(sessionStore);
var localServices = LocalPrincipalServices.Create(principalStore);
var authorization = new FrameworkAuthorizationService([new BuiltInAuthorizationPolicy()]);
var apiPermission = new PrincipalPermission("api.use");

var admin = new Principal(
	PrincipalId.New(),
	"JWT Admin",
	"jwt-admin@example.test",
	Roles: new HashSet<PrincipalRole> { new("admin") },
	Permissions: new HashSet<PrincipalPermission> { apiPermission }
);

principalStore.Add(admin, "admin");
await localServices.CredentialService.CreateAsync("admin", "password", admin.Id);

var googleClientId =
	builder.Configuration["Google:ClientId"] ?? Environment.GetEnvironmentVariable("LEVA_GOOGLE_CLIENT_ID");
var googleClientSecret =
	builder.Configuration["Google:ClientSecret"] ?? Environment.GetEnvironmentVariable("LEVA_GOOGLE_CLIENT_SECRET");
var googleConfigured = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);

builder.Services.AddSingleton<IClock, FrameworkSystemClock>();
builder.Services.AddSingleton<IPrincipalStore>(principalStore);
builder.Services.AddSingleton<ILocalCredentialStore>(localServices.Credentials);
builder.Services.AddSingleton(localServices.CredentialService);
builder.Services.AddSingleton<IAuthSessionStore>(sessionStore);
builder.Services.AddSingleton(sessionService);
builder.Services.AddSingleton(authorization);

// AuthenticationService is composed from framework policies registered in DI. Local credentials issue
// the sample JWT, while Google is added only when the external OAuth client is configured.
builder.Services.AddSingleton<AuthenticationPolicy>(localServices.AuthenticationPolicy);
builder.Services.AddSingleton<FrameworkAuthenticationService>(provider => new FrameworkAuthenticationService(
	provider.GetServices<AuthenticationPolicy>().ToArray(),
	sessionService
));

// Cookie/session identity is still registered so Google callback can sign the user into the browser.
builder.Services.AddAspNetPrincipalServices(options =>
{
	options.AllowHeaderSession = true;
	options.SecureCookie = false;
});

// The JWT provider issues and validates compact bearer tokens from framework auth sessions.
builder.Services.AddAspNetJwtPrincipalServices(options =>
{
	options.Issuer = "Leva.Framework.Sample.WebIdentity";
	options.Audience = "Leva.Framework.Sample.WebIdentity";
	options.SigningKey = "sample-development-signing-key-32-bytes-minimum";
	options.TokenLifetime = TimeSpan.FromMinutes(30);
});

if (googleConfigured)
{
	builder.Services.AddAspNetGooglePrincipalServices(options =>
	{
		options.ClientId = googleClientId!;
		options.ClientSecret = googleClientSecret!;
		options.SecureCookie = false;
		options.DefaultReturnUrl = "/";
	});
}

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(
		"JwtApiUse",
		policy =>
		{
			// This endpoint must be authenticated by the framework JWT handler, not by the cookie handler.
			policy.AuthenticationSchemes.Add(AspNetJwtDefaults.AuthenticationScheme);
			policy.Requirements.Add(
				new AspNetAuthorizationRequirement(AuthorizationRequirement.Permission(apiPermission))
			);
		}
	);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles(
	new StaticFileOptions
	{
		OnPrepareResponse = context =>
		{
			context.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
			context.Context.Response.Headers.Pragma = "no-cache";
			context.Context.Response.Headers.Expires = "0";
		},
	}
);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/favicon.ico", () => Results.NoContent());
app.MapPost(
	"/sample/logout",
	(HttpContext context) =>
	{
		context.Response.Cookies.Delete(AspNetDefaults.SessionCookieName);
		return Results.Ok(new { cleared = true });
	}
);

// The built-in JWT endpoint authenticates local credentials and returns a bearer token.
app.MapAspNetJwtEndpoints();

if (googleConfigured)
	// Real Google login needs an external client id/secret and callback URL registered in Google Cloud.
	app.MapAspNetGoogleEndpoints();
else
{
	app.MapGet(
		"/identity/google/login",
		() => Results.BadRequest(new { message = "Google auth is not configured for this sample host." })
	);
	app.MapGet(
		"/identity/google/callback",
		() => Results.BadRequest(new { message = "Google auth is not configured for this sample host." })
	);
}

app.MapGet(
	"/sample/google/status",
	(HttpContext context) =>
	{
		var callbackUrl = $"{context.Request.Scheme}://{context.Request.Host}/identity/google/callback";
		return new WebIdentityGoogleStatus(
			googleConfigured,
			"/identity/google/login",
			callbackUrl,
			googleConfigured
				? "Google auth is configured. The login button will start the real OAuth flow."
				: "Google auth is not configured. Set LEVA_GOOGLE_CLIENT_ID and LEVA_GOOGLE_CLIENT_SECRET to try the real OAuth flow."
		);
	}
);

app.MapGet(
		"/api/jwt/admin-area",
		(HttpContext context) =>
		{
			var roles = context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
			var permissions = context
				.User.FindAll(AspNetClaimsPrincipalMapper.PermissionClaimType)
				.Select(claim => claim.Value)
				.ToArray();

			return new WebIdentityApiResponse(
				"JWT bearer token was accepted by ASP.NET Core and authorized by framework permission.",
				context.User.Identity?.Name,
				context.User.Identity?.IsAuthenticated == true,
				roles,
				permissions
			);
		}
	)
	.RequireAuthorization("JwtApiUse");

app.Lifetime.ApplicationStarted.Register(() =>
{
	Console.WriteLine("WebIdentity sample");
	Console.WriteLine("Open the printed local URL, run JWT checks, and optionally configure Google OAuth.");
});

app.Run();
