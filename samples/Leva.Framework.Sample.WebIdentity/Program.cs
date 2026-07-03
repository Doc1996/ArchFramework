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

// Samples may run as Production; load user secrets explicitly for local Google OAuth.
builder.Configuration.AddUserSecrets<Program>(optional: true);

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

// Google can be configured through user secrets, .NET environment variables, or explicit environment variables.
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

// Compose framework authentication policies for the ASP.NET adapters.
builder.Services.AddSingleton<AuthenticationPolicy>(localServices.AuthenticationPolicy);
builder.Services.AddSingleton<FrameworkAuthenticationService>(provider => new FrameworkAuthenticationService(
	provider.GetServices<AuthenticationPolicy>().ToArray(),
	sessionService
));

// Cookie/session identity lets the Google callback sign the user into the browser.
builder.Services.AddAspNetPrincipalServices(options =>
{
	options.AllowHeaderSession = true;
	options.SecureCookie = false;
});

// The JWT provider issues and validates bearer tokens from framework auth sessions.
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
			// Require JWT authentication here, not the cookie handler.
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
			// Samples change often while developing, so disable browser caching for local static files.
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
	// Real Google login needs a configured OAuth client and registered callback URL.
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
				: "Google auth is not configured. Set Google:ClientId and Google:ClientSecret with user secrets, or set LEVA_GOOGLE_CLIENT_ID and LEVA_GOOGLE_CLIENT_SECRET."
		);
	}
);

app.MapGet(
	"/sample/google/principal",
	(HttpContext context) =>
	{
		var roles = context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
		var permissions = context
			.User.FindAll(AspNetClaimsPrincipalMapper.PermissionClaimType)
			.Select(claim => claim.Value)
			.ToArray();
		var name = context.User.Identity?.Name ?? context.User.FindFirst("name")?.Value;
		var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? context.User.FindFirst("email")?.Value;

		return new WebIdentityGooglePrincipal(
			context.User.Identity?.IsAuthenticated == true,
			name,
			email,
			roles,
			permissions
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
