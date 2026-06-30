using Leva.Framework.Identity.Memory;
using Leva.Framework.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Identity.AspNet.Tests;

public sealed class AspNetAuthorizationHandlerTests
{
	[Fact]
	public async Task HandleAsync_SucceedsWhenFrameworkAuthorizationSucceeds()
	{
		var sessionService = new AuthSessionService(new MemoryAuthSessionStore());
		var authorization = new AuthorizationService([new BuiltInAuthorizationPolicy()]);
		var principalMapper = new AspNetClaimsPrincipalMapper(Options.Create(new AspNetIdentityOptions()));

		var principal = new Principal(
			new PrincipalId("principal-1"),
			"User One",
			Permissions: [new PrincipalPermission("plans.create")]
		);

		var session = ResultAssert.Success(await sessionService.CreateAsync(principal));
		var requirement = new AspNetAuthorizationRequirement(
			AuthorizationRequirement.Permission(new PrincipalPermission("plans.create"))
		);
		var context = new AuthorizationHandlerContext([requirement], principalMapper.Map(session), null);
		var handler = new AspNetAuthorizationHandler(sessionService, authorization, principalMapper);

		await handler.HandleAsync(context);
		Assert.True(context.HasSucceeded);
	}
}
