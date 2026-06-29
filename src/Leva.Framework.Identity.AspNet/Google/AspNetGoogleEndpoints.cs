using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps built in ASP.NET Core Google sign-in endpoints.
/// </summary>
public static class AspNetGoogleEndpoints
{
	public static RouteGroupBuilder MapAspNetGoogleEndpoints(
		this IEndpointRouteBuilder endpoints,
		string prefix = "/identity/google"
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);
		var group = endpoints.MapGroup(prefix);

		group.MapGet("/login", StartAsync);
		group.MapGet("/callback", CompleteAsync);
		return group;
	}

	private static IResult StartAsync(
		HttpContext context,
		AspNetGoogleSignInService google,
		string? returnUrl = null
	) => Results.Redirect(google.CreateAuthorizationUrl(context, returnUrl));

	private static async Task<IResult> CompleteAsync(
		HttpContext context,
		AspNetGoogleSignInService google,
		CancellationToken token
	)
	{
		var result = await google.CompleteSignInAsync(context, token);
		return result.IsSuccess ? Results.Redirect(result.Value!) : Results.BadRequest(result.Error);
	}
}
