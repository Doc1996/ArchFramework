using Leva.Framework.Core;
using Leva.Framework.Identity;
using Leva.Framework.Identity.AspNet;
using Leva.Framework.Identity.Local;
using Leva.Framework.Identity.Memory;
using FrameworkAuthenticationService = Leva.Framework.Identity.AuthenticationService;
using FrameworkAuthorizationService = Leva.Framework.Identity.AuthorizationService;
using FrameworkSystemClock = Leva.Framework.Core.SystemClock;

var builder = WebApplication.CreateBuilder(args);

// Memory stores keep the sample self-contained; a real app would replace these with durable stores.
var principalStore = new MemoryPrincipalStore();
var sessionStore = new MemoryAuthSessionStore();
var sessionService = new AuthSessionService(sessionStore);

// LocalPrincipalServices groups the local credential store, secret protection, credential service, and local auth policy.
var localServices = LocalPrincipalServices.Create(principalStore);

// AuthenticationService receives one or more policies; here only the local-secret policy is enabled.
var authentication = new FrameworkAuthenticationService([localServices.AuthenticationPolicy], sessionService);

// AuthorizationService stays framework-level; ASP.NET Core only adapts requests to this service.
var authorization = new FrameworkAuthorizationService([new BuiltInAuthorizationPolicy()]);
var adminPermission = new PrincipalPermission("account.manage");

var admin = new Principal(
	PrincipalId.New(),
	"Local Admin",
	"admin@example.test",
	Roles: new HashSet<PrincipalRole> { new("admin") },
	Permissions: new HashSet<PrincipalPermission> { adminPermission }
);
var operatorUser = new Principal(
	PrincipalId.New(),
	"Local Operator",
	"operator@example.test",
	Roles: new HashSet<PrincipalRole> { new("operator") }
);

// The sample creates two principals: admin can access the protected endpoint; operator can log in but cannot.
principalStore.Add(admin, "admin");
principalStore.Add(operatorUser, "operator");
await localServices.CredentialService.CreateAsync("admin", "password", admin.Id);
await localServices.CredentialService.CreateAsync("operator", "password", operatorUser.Id);

// Register the same framework service instances that were composed above so ASP.NET endpoints use them.
builder.Services.AddSingleton<IClock, FrameworkSystemClock>();
builder.Services.AddSingleton<IPrincipalStore>(principalStore);
builder.Services.AddSingleton<ILocalCredentialStore>(localServices.Credentials);
builder.Services.AddSingleton(localServices.CredentialService);
builder.Services.AddSingleton(localServices.AuthenticationPolicy);
builder.Services.AddSingleton<IAuthSessionStore>(sessionStore);
builder.Services.AddSingleton(sessionService);
builder.Services.AddSingleton(authentication);
builder.Services.AddSingleton(authorization);

// AddAspNetPrincipalServices adapts the framework identity services to ASP.NET Core auth middleware.
builder.Services.AddAspNetPrincipalServices(options =>
{
	options.AllowHeaderSession = true;
	options.SecureCookie = false;
});

// ASP.NET Core owns endpoint policies, but the requirement itself uses framework permission concepts.
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(
		"AccountManage",
		policy =>
			policy.Requirements.Add(
				// The ASP.NET policy delegates the actual permission requirement to Leva.Framework.Identity.
				new AspNetAuthorizationRequirement(AuthorizationRequirement.Permission(adminPermission))
			)
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

// ASP.NET Core middleware reads the framework auth session cookie and populates HttpContext.User.
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/favicon.ico", () => Results.NoContent());

// This sample helper clears only the framework auth-session cookie so browser checks can start from anonymous state.
app.MapPost(
	"/sample/logout",
	(HttpContext context) =>
	{
		context.Response.Cookies.Delete(AspNetDefaults.SessionCookieName);
		return Results.Ok(new { cleared = true });
	}
);

// The framework extension exposes login and current-principal endpoints for this sample host.
app.MapAspNetIdentityEndpoints();

// RequireAuthorization proves that ASP.NET Core middleware can enforce a framework permission.
app.MapGet(
		"/account/admin-area",
		(HttpContext context) =>
			Results.Ok(
				new
				{
					message = $"Hello {context.User.Identity?.Name ?? "unknown"}.",
					isAuthenticated = context.User.Identity?.IsAuthenticated == true,
					requiredPermission = "account.manage",
				}
			)
	)
	.RequireAuthorization("AccountManage");

app.Lifetime.ApplicationStarted.Register(() =>
{
	Console.WriteLine("LocalIdentity sample");
	Console.WriteLine(
		"Open the printed local URL and click Run all checks, or use the curl commands from the sample README."
	);
});

app.Run();
