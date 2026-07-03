using Leva.Framework.Sample.StorageDesk;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// The host stays thin; StorageDeskRunner composes identity, notifications, and storage providers.
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

app.MapPost(
	"/storagedesk/run",
	async (CancellationToken token) =>
	{
		// The browser triggers the same application scenario against every storage provider.
		var result = await StorageDeskRunner.RunAsync(token);
		return Results.Ok(result);
	}
);

app.Lifetime.ApplicationStarted.Register(() =>
{
	Console.WriteLine("StorageDesk sample");
	Console.WriteLine("Open the printed local URL and click Run storage desk checks.");
});

app.Run();
