using Microsoft.AspNetCore.Authorization;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Delegates ASP.NET Core authorization requirements to the framework authorization service.
/// </summary>
public sealed class AspNetAuthorizationHandler(
	AspNetAuthSessionSource sessionSource,
	AuthorizationService authorization
) : AuthorizationHandler<AspNetAuthorizationRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		AspNetAuthorizationRequirement requirement
	)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(requirement);

		var session = await sessionSource.GetSessionAsync();
		if (session.IsFailure)
			return;

		var request = new AuthorizationRequest(session.Value?.Principal, requirement.Requirement, session.Value);
		var result = await authorization.AuthorizeAsync(request);

		if (result.IsSuccess && result.Value is { IsAuthorized: true })
			context.Succeed(requirement);
	}
}
