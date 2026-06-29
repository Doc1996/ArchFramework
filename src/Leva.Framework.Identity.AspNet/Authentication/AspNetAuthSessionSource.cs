using Leva.Framework.Core;
using Microsoft.AspNetCore.Http;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Resolves the current framework auth session from the ASP.NET Core HTTP context.
/// </summary>
public sealed class AspNetAuthSessionSource(
	IHttpContextAccessor contextAccessor,
	AspNetAuthSessionReader sessionReader,
	AuthSessionService sessionService
) : AuthSessionSource
{
	public override async Task<Result<AuthSession?>> GetSessionAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var context = contextAccessor.HttpContext;
		if (context is null)
			return Result<AuthSession?>.Ok(null);

		var sessionId = sessionReader.Read(context);
		return sessionId is null
			? Result<AuthSession?>.Ok(null)
			: await sessionService.LoadAsync(sessionId.Value, token);
	}
}
