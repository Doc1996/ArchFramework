using Leva.Framework.Core;
using Microsoft.AspNetCore.Http;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Provides high-level ASP.NET Core identity operations for built in endpoints and custom web UI.
/// </summary>
public sealed class AspNetIdentityService(
	AuthenticationService authentication,
	AuthSessionService sessionService,
	AspNetAuthSessionReader sessionReader,
	AspNetAuthSessionWriter sessionWriter
)
{
	public async Task<Result<AuthenticationResult>> LoginAsync(
		HttpContext context,
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(request);
		token.ThrowIfCancellationRequested();

		var result = await authentication.AuthenticateAsync(request, token);
		if (result.IsFailure)
			return result;

		var authenticationResult = result.Value!;
		if (authenticationResult.Session is not null)
			sessionWriter.Write(context, authenticationResult.Session.SessionId);

		return result;
	}

	public async Task<Result> LogoutAsync(HttpContext context, CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(context);
		token.ThrowIfCancellationRequested();

		var sessionId = sessionReader.Read(context);
		if (sessionId is null)
			return Result.Fail(PrincipalErrors.Unauthorized("Auth session is missing."));

		var result = await authentication.SignOutAsync(sessionId.Value, token);
		if (result.IsSuccess)
			sessionWriter.Delete(context);

		return result;
	}

	public async Task<Result<AuthSession?>> GetSessionAsync(HttpContext context, CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(context);
		token.ThrowIfCancellationRequested();

		var sessionId = sessionReader.Read(context);
		if (sessionId is null)
			return Result<AuthSession?>.Ok(null);

		return await sessionService.LoadAsync(sessionId.Value, token);
	}

	public async Task<Result<Principal?>> GetPrincipalAsync(HttpContext context, CancellationToken token = default)
	{
		var session = await GetSessionAsync(context, token);
		if (session.IsFailure)
			return Result<Principal?>.Fail(session.Error);

		return Result<Principal?>.Ok(session.Value?.Principal);
	}
}
