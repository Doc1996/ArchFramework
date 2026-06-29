using Leva.Framework.Identity.Memory;
using Xunit;

namespace Leva.Framework.Identity.Local.Tests;

public sealed class LocalPrincipalServicesTests
{
	[Fact]
	public async Task Create_ComposesCredentialServiceAndAuthenticationPolicy()
	{
		var principals = new MemoryPrincipalStore();
		var principal = new Principal(new PrincipalId("principal-1"), "User One");

		principals.Add(principal, "user");
		var services = LocalPrincipalServices.Create(
			principals,
			secretProtector: new LocalSecretProtector(1_000, 16, 32)
		);

		ResultAssert.Success(await services.CredentialService.CreateAsync("user", "secret", principal.Id));
		var result = await services.AuthenticationPolicy.AuthenticateAsync(
			new AuthenticationRequest(LocalAuthenticationMethods.Secret, "user", "secret")
		);

		var authentication = ResultAssert.Success(result);
		Assert.True(authentication.IsAuthenticated);
		Assert.Equal(principal, authentication.Principal);
	}
}
