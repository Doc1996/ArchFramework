using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps built in ASP.NET Core identity endpoints for login, logout, and current principal lookup.
/// </summary>
public static class AspNetIdentityEndpoints
{
	public static RouteGroupBuilder MapAspNetIdentityEndpoints(
		this IEndpointRouteBuilder endpoints,
		string prefix = "/identity"
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);
		var group = endpoints.MapGroup(prefix);

		group.MapPost("/login", SignInAsync);
		group.MapPost("/logout", SignOutAsync);
		group.MapGet("/me", GetCurrentPrincipalAsync);

		return group;
	}

	private static async Task<IResult> SignInAsync(
		HttpContext context,
		AspNetLoginRequest request,
		AspNetSignInService signIn,
		CancellationToken token
	)
	{
		var result = await signIn.SignInAsync(context, request.ToAuthenticationRequest(), token);
		if (result.IsFailure)
			return Results.BadRequest(result.Error);

		var response = AspNetLoginResponse.From(result.Value!);
		return response.IsAuthenticated ? Results.Ok(response) : Results.Unauthorized();
	}

	private static async Task<IResult> SignOutAsync(
		HttpContext context,
		AspNetSignInService signIn,
		CancellationToken token
	)
	{
		var result = await signIn.SignOutAsync(context, token);
		return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
	}

	private static async Task<IResult> GetCurrentPrincipalAsync(
		AspNetAuthSessionSource sessionSource,
		CancellationToken token
	)
	{
		var session = await sessionSource.GetSessionAsync(token);
		if (session.IsFailure)
			return Results.BadRequest(session.Error);

		return Results.Ok(
			session.Value is null
				? AspNetCurrentPrincipalResponse.Anonymous
				: AspNetCurrentPrincipalResponse.From(session.Value)
		);
	}
}
