using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps built in ASP.NET Core Google endpoints: GET /identity/google/login and GET /identity/google/callback.
/// </summary>
public static class AspNetGoogleEndpoints
{
	public const string DefaultPrefix = "/identity/google";
	public const string LoginPath = "/login";
	public const string CallbackPath = "/callback";

	public static RouteGroupBuilder MapAspNetGoogleEndpoints(
		this IEndpointRouteBuilder endpoints,
		string prefix = DefaultPrefix
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);
		var group = endpoints.MapGroup(prefix);

		group.MapGet(LoginPath, LoginAsync);
		group.MapGet(CallbackPath, CallbackAsync);
		return group;
	}

	private static IResult LoginAsync(HttpContext context, AspNetGoogleSignInService google, string? returnUrl = null)
	{
		return Results.Redirect(google.CreateAuthorizationUrl(context, returnUrl));
	}

	private static async Task<IResult> CallbackAsync(
		HttpContext context,
		AspNetGoogleSignInService google,
		CancellationToken token
	)
	{
		var result = await google.CompleteLoginAsync(context, token);
		return result.IsSuccess ? Results.Redirect(result.Value!) : Results.BadRequest(result.Error);
	}
}
