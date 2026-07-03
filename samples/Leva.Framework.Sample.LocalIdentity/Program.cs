using Leva.Framework.Core;
using Leva.Framework.Identity;
using Leva.Framework.Identity.AspNet;
using Leva.Framework.Identity.Local;
using Leva.Framework.Identity.Memory;
using FrameworkAuthenticationService = Leva.Framework.Identity.AuthenticationService;
using FrameworkAuthorizationService = Leva.Framework.Identity.AuthorizationService;
using FrameworkSystemClock = Leva.Framework.Core.SystemClock;

var builder = WebApplication.CreateBuilder(args);

// Memory stores keep the sample self-contained.
var principalStore = new MemoryPrincipalStore();
var sessionStore = new MemoryAuthSessionStore();
var sessionService = new AuthSessionService(sessionStore);

// LocalPrincipalServices groups the local credential pieces used by the sample.
var localServices = LocalPrincipalServices.Create(principalStore);

// AuthenticationService uses the local-secret policy in this sample.
var authentication = new FrameworkAuthenticationService([localServices.AuthenticationPolicy], sessionService);

// Authorization stays framework-level; ASP.NET Core adapts requests to it.
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

// Admin can access the protected endpoint; operator can log in but cannot.
principalStore.Add(admin, "admin");
principalStore.Add(operatorUser, "operator");
await localServices.CredentialService.CreateAsync("admin", "password", admin.Id);
await localServices.CredentialService.CreateAsync("operator", "password", operatorUser.Id);

// Register the composed framework services for the ASP.NET adapters.
builder.Services.AddSingleton<IClock, FrameworkSystemClock>();
builder.Services.AddSingleton<IPrincipalStore>(principalStore);
builder.Services.AddSingleton<ILocalCredentialStore>(localServices.Credentials);
builder.Services.AddSingleton(localServices.CredentialService);
builder.Services.AddSingleton(localServices.AuthenticationPolicy);
builder.Services.AddSingleton<IAuthSessionStore>(sessionStore);
builder.Services.AddSingleton(sessionService);
builder.Services.AddSingleton(authentication);
builder.Services.AddSingleton(authorization);

// Adapt framework identity services to ASP.NET Core auth middleware.
builder.Services.AddAspNetPrincipalServices(options =>
{
	options.AllowHeaderSession = true;
	options.SecureCookie = false;
});

// ASP.NET Core owns the policy; the requirement uses framework permission concepts.
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(
		"AccountManage",
		policy =>
			policy.Requirements.Add(
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

// Middleware reads the framework session cookie and populates HttpContext.User.
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/favicon.ico", () => Results.NoContent());

// Helper endpoint clears the framework auth-session cookie for repeatable checks.
app.MapPost(
	"/sample/logout",
	(HttpContext context) =>
	{
		context.Response.Cookies.Delete(AspNetDefaults.SessionCookieName);
		return Results.Ok(new { cleared = true });
	}
);

// Expose framework login and current-principal endpoints.
app.MapAspNetIdentityEndpoints();

// Prove that ASP.NET Core middleware can enforce a framework permission.
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
