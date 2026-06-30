using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps built in ASP.NET Core JWT endpoints: POST /identity/jwt/login.
/// </summary>
public static class AspNetJwtEndpoints
{
	public const string DefaultPrefix = "/identity/jwt";
	public const string LoginPath = "/login";

	public static RouteGroupBuilder MapAspNetJwtEndpoints(
		this IEndpointRouteBuilder endpoints,
		string prefix = DefaultPrefix
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);
		var group = endpoints.MapGroup(prefix);

		group.MapPost(LoginPath, LoginAsync);
		return group;
	}

	private static async Task<IResult> LoginAsync(
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
