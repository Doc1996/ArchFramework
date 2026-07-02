using Leva.Framework.Core;
using Leva.Framework.Notifications;
using Leva.Framework.Notifications.Memory;
using Leva.Framework.Notifications.SignalR;
using Leva.Framework.Sample.LiveDashboard;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// This is sample-host behavior, not framework behavior. A real application should choose
// its own graceful shutdown timeout for active SignalR/websocket connections.
builder.Services.Configure<HostOptions>(options =>
{
	options.ShutdownTimeout = TimeSpan.FromSeconds(2);
});

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();

// SignalR is the live delivery provider. It registers the framework INotificationGateway.
builder.Services.AddSignalRNotifications();

// The browser history deliberately uses one concrete memory store that is also exposed through INotificationStore.
builder.Services.AddSingleton<MemoryNotificationStore>();
builder.Services.AddSingleton<INotificationStore>(provider => provider.GetRequiredService<MemoryNotificationStore>());
builder.Services.AddSingleton<DashboardNotificationHistory>();

// The application endpoint uses NotificationService. It sends through SignalRNotificationGateway and produces
// NotificationEntry values; DashboardNotificationHistory records those entries for the visible sample history.
builder.Services.AddSingleton<NotificationService>();

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
app.MapGet("/favicon.ico", () => Results.NoContent());

// MapSignalRNotifications exposes the framework notification hub used by the browser page.
app.MapSignalRNotifications();

app.MapPost(
	"/dashboard/notify",
	async (NotificationService notifications, DashboardNotificationHistory history, CancellationToken token) =>
	{
		var result = await SendAndRecordDemoNotificationAsync(notifications, history, "Live update", token);
		return result.IsSuccess
			? Results.Ok(DashboardNotificationEntry.From(result.Value!))
			: Results.BadRequest(result.Error);
	}
);

app.MapPost(
	"/dashboard/self-check",
	async (NotificationService notifications, DashboardNotificationHistory history, CancellationToken token) =>
	{
		var before = history.Entries.Count;
		var result = await SendAndRecordDemoNotificationAsync(notifications, history, "Self-check", token);
		var after = history.Entries.Count;
		var passed = result.IsSuccess && after == before + 1;

		return Results.Ok(
			new DashboardSelfCheck(
				passed,
				passed ? "notification was sent through SignalR and stored"
					: result.IsFailure ? result.Error.Message
					: "notification was not stored as expected",
				before,
				after
			)
		);
	}
);

app.MapGet(
	"/dashboard/history",
	(DashboardNotificationHistory history) =>
		// The browser receives a DTO instead of raw framework NotificationEntry objects.
		history.Snapshot()
);

app.Lifetime.ApplicationStarted.Register(() =>
{
	Console.WriteLine("LiveDashboard sample");
	Console.WriteLine(
		"Open the printed local URL, then use the page buttons to send notifications and run the self-check."
	);
	Console.WriteLine("Ctrl+C waits at most two seconds for active SignalR connections in this sample host.");
});

app.Run();

static async Task<Result<NotificationEntry>> SendAndRecordDemoNotificationAsync(
	NotificationService notifications,
	DashboardNotificationHistory history,
	string subject,
	CancellationToken token
)
{
	// NotificationService owns the reusable notification flow: create notification, call gateway, and create entry.
	var result = await notifications.SendAsync(
		new NotificationRecipient("demo", DisplayName: "Demo user"),
		SignalRNotificationOptions.DefaultChannel,
		subject,
		$"Server time is {DateTimeOffset.UtcNow:HH:mm:ss} UTC.",
		token
	);

	if (result.IsSuccess)
		await history.RecordAsync(result.Value!, token);

	return result;
}
