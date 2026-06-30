using Microsoft.AspNetCore.Authorization;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Delegates ASP.NET Core authorization requirements to the framework authorization service.
/// </summary>
public sealed class AspNetAuthorizationHandler(
	AuthSessionService sessionService,
	AuthorizationService authorization,
	AspNetClaimsPrincipalMapper principalMapper
) : AuthorizationHandler<AspNetAuthorizationRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		AspNetAuthorizationRequirement requirement
	)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(requirement);

		var sessionId = principalMapper.GetSessionId(context.User);
		if (sessionId is null)
			return;

		var session = await sessionService.LoadAsync(sessionId.Value);
		if (session.IsFailure)
			return;

		if (session.Value is not { SessionStatus: AuthSessionStatus.Active } activeSession)
			return;

		var request = new AuthorizationRequest(activeSession.Principal, requirement.Requirement, activeSession);
		var result = await authorization.AuthorizeAsync(request);

		if (result.IsSuccess && result.Value is { IsAuthorized: true })
			context.Succeed(requirement);
	}
}
