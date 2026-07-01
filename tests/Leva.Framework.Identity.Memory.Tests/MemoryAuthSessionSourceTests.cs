using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

public sealed class MemoryAuthSessionSourceTests
{
	[Fact]
	public async Task GetSessionAsync_ReturnsNullWhenNoSessionIsConfigured()
	{
		var services = MemoryPrincipalServices.Create();
		var result = await services.SessionSource.GetSessionAsync();
		var session = ResultAssert.Success(result);

		Assert.Null(session);
	}

	[Fact]
	public async Task GetSessionAsync_LoadsConfiguredSession()
	{
		var services = MemoryPrincipalServices.Create();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");
		var session = ResultAssert.Success(await services.SessionService.CreateAsync(principal));

		services.SessionSource.SetSession(session.SessionId);
		var result = await services.SessionSource.GetSessionAsync();
		var loaded = ResultAssert.Success(result);

		Assert.Equal(session.SessionId, loaded?.SessionId);
	}

	[Fact]
	public async Task PrincipalAccess_UsesMemorySessionSourceAndAuthorization()
	{
		var services = MemoryPrincipalServices.Create();
		var principal = new Principal(
			new PrincipalId("principal-1"),
			"User One",
			Permissions: new HashSet<PrincipalPermission> { new("plans.edit") }
		);

		var session = ResultAssert.Success(await services.SessionService.CreateAsync(principal));
		services.SessionSource.SetSession(session.SessionId);

		var result = await services.Access.RequireAsync(new PrincipalPermission("plans.edit"));
		var authorization = ResultAssert.Success(result);
		Assert.True(authorization.IsAuthorized);
	}
}
