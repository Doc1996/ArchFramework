using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps built in ASP.NET Core identity endpoints: POST /identity/login, POST /identity/logout, and GET /identity/principal.
/// </summary>
public static class AspNetIdentityEndpoints
{
	public const string DefaultPrefix = "/identity";
	public const string LoginPath = "/login";
	public const string LogoutPath = "/logout";
	public const string PrincipalPath = "/principal";

	public static RouteGroupBuilder MapAspNetIdentityEndpoints(
		this IEndpointRouteBuilder endpoints,
		string prefix = DefaultPrefix
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);
		var group = endpoints.MapGroup(prefix);

		group.MapPost(LoginPath, LoginAsync);
		group.MapPost(LogoutPath, LogoutAsync);
		group.MapGet(PrincipalPath, GetPrincipalAsync);

		return group;
	}

	private static async Task<IResult> LoginAsync(
		HttpContext context,
		AspNetLoginRequest request,
		AspNetIdentityService identity,
		CancellationToken token
	)
	{
		var result = await identity.LoginAsync(context, request.ToAuthenticationRequest(), token);
		if (result.IsFailure)
			return Results.BadRequest(result.Error);

		var response = AspNetLoginResponse.FromAuthenticationResult(result.Value!);
		return response.IsAuthenticated ? Results.Ok(response) : Results.Unauthorized();
	}

	private static async Task<IResult> LogoutAsync(
		HttpContext context,
		AspNetIdentityService identity,
		CancellationToken token
	)
	{
		var result = await identity.LogoutAsync(context, token);
		return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
	}

	private static async Task<IResult> GetPrincipalAsync(
		HttpContext context,
		AspNetIdentityService identity,
		CancellationToken token
	)
	{
		var session = await identity.GetSessionAsync(context, token);
		if (session.IsFailure)
			return Results.BadRequest(session.Error);

		var response = session.Value is null
			? AspNetPrincipalResponse.Anonymous
			: AspNetPrincipalResponse.FromSession(session.Value);
		return Results.Ok(response);
	}
}
