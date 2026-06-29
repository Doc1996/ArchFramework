using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps built in ASP.NET Core JWT endpoints for token issuing.
/// </summary>
public static class AspNetJwtEndpoints
{
	public static RouteGroupBuilder MapAspNetJwtEndpoints(
		this IEndpointRouteBuilder endpoints,
		string prefix = "/identity/jwt"
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var group = endpoints.MapGroup(prefix);
		group.MapPost("/login", SignInAsync);
		return group;
	}

	private static async Task<IResult> SignInAsync(
		AspNetLoginRequest request,
		AuthenticationService authentication,
		AspNetJwtTokenService tokens,
		CancellationToken token
	)
	{
		var result = await authentication.AuthenticateAsync(request.ToAuthenticationRequest(), token);
		if (result.IsFailure)
			return Results.BadRequest(result.Error);

		var authenticationResult = result.Value!;
		if (!authenticationResult.IsAuthenticated || authenticationResult.Session is null)
			return Results.Unauthorized();

		var tokenResult = tokens.CreateToken(authenticationResult.Session);
		return tokenResult.IsSuccess ? Results.Ok(tokenResult.Value) : Results.BadRequest(tokenResult.Error);
	}
}
