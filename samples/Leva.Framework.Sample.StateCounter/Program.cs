using Leva.Framework.Sample.StateCounter;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// The host is intentionally thin; the sample workflow composition lives in StateCounterRunner.
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
	"/workflow/run",
	async (CancellationToken token) =>
	{
		// The browser calls one endpoint so the sample can show the complete Engine and Execution flow.
		var result = await StateCounterRunner.RunAsync(token);
		return Results.Ok(result);
	}
);

app.Lifetime.ApplicationStarted.Register(() =>
{
	Console.WriteLine("StateCounter sample");
	Console.WriteLine("Open the printed local URL and click Run workflow checks.");
});

app.Run();
