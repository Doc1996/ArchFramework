using Leva.Framework.Core;
using Microsoft.AspNetCore.Http;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Signs principals in and out through framework authentication services and ASP.NET Core auth session cookies.
/// </summary>
public sealed class AspNetSignInService(
	AuthenticationService authentication,
	AspNetAuthSessionReader sessionReader,
	AspNetAuthSessionWriter sessionWriter
)
{
	public async Task<Result<AuthenticationResult>> SignInAsync(
		HttpContext context,
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(request);

		var result = await authentication.AuthenticateAsync(request, token);
		if (result.IsFailure)
			return result;

		var authenticationResult = result.Value!;
		if (authenticationResult.Session is not null)
			sessionWriter.Write(context, authenticationResult.Session.SessionId);

		return result;
	}

	public async Task<Result> SignOutAsync(HttpContext context, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(context);

		var sessionId = sessionReader.Read(context);
		if (sessionId is null)
			return Result.Fail(PrincipalErrors.Unauthorized("Auth session is missing."));

		var result = await authentication.SignOutAsync(sessionId.Value, token);
		if (result.IsSuccess)
			sessionWriter.Delete(context);

		return result;
	}
}
