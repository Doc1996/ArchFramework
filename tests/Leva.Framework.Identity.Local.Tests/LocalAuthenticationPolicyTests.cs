using Leva.Framework.Identity.Memory;
using Xunit;

namespace Leva.Framework.Identity.Local.Tests;

public sealed class LocalAuthenticationPolicyTests
{
	[Fact]
	public async Task AuthenticateAsync_AuthenticatesValidSecret()
	{
		var principals = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		principals.Add(principal, "user");

		var credentials = new MemoryLocalCredentialStore();
		var secretProtector = TestSecretProtector();
		var credentialService = new LocalCredentialService(credentials, secretProtector);
		ResultAssert.Success(await credentialService.CreateAsync("user", "secret", principal.Id));

		var policy = new LocalAuthenticationPolicy(credentials, principals, secretProtector);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(LocalAuthenticationMethods.Secret, "user", "secret")
		);

		var authentication = ResultAssert.Success(result);
		Assert.True(authentication.IsAuthenticated);
		Assert.Equal(principal, authentication.Principal);
	}

	[Fact]
	public async Task AuthenticateAsync_FailsInvalidSecret()
	{
		var principals = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		principals.Add(principal, "user");

		var credentials = new MemoryLocalCredentialStore();
		var secretProtector = TestSecretProtector();
		var credentialService = new LocalCredentialService(credentials, secretProtector);
		ResultAssert.Success(await credentialService.CreateAsync("user", "secret", principal.Id));

		var policy = new LocalAuthenticationPolicy(credentials, principals, secretProtector);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(LocalAuthenticationMethods.Secret, "user", "wrong")
		);

		var authentication = ResultAssert.Success(result);
		Assert.False(authentication.IsAuthenticated);
		Assert.Null(authentication.Principal);
	}

	[Fact]
	public async Task AuthenticateAsync_FailsDisabledCredential()
	{
		var principals = new MemoryPrincipalStore();
		var principal = TestPrincipal();
		principals.Add(principal, "user");

		var credentials = new MemoryLocalCredentialStore();
		var secretProtector = TestSecretProtector();
		var credentialService = new LocalCredentialService(credentials, secretProtector);

		ResultAssert.Success(await credentialService.CreateAsync("user", "secret", principal.Id));
		ResultAssert.Success(await credentialService.DisableAsync("user"));

		var policy = new LocalAuthenticationPolicy(credentials, principals, secretProtector);
		var result = await policy.AuthenticateAsync(
			new AuthenticationRequest(LocalAuthenticationMethods.Secret, "user", "secret")
		);

		var authentication = ResultAssert.Success(result);
		Assert.False(authentication.IsAuthenticated);
	}

	[Fact]
	public async Task AuthenticationService_CreatesSessionForLocalAuthentication()
	{
		var services = MemoryPrincipalServices.Create();
		var principal = TestPrincipal();
		services.Principals.Add(principal, "user");

		var local = LocalPrincipalServices.Create(services.Principals, secretProtector: TestSecretProtector());
		ResultAssert.Success(await local.CredentialService.CreateAsync("user", "secret", principal.Id));

		var authentication = new AuthenticationService([local.AuthenticationPolicy], services.SessionService);
		var result = await authentication.AuthenticateAsync(
			new AuthenticationRequest(LocalAuthenticationMethods.Secret, "user", "secret")
		);

		var authenticated = ResultAssert.Success(result);
		Assert.True(authenticated.IsAuthenticated);
		Assert.NotNull(authenticated.Session);
		Assert.Equal(principal, authenticated.Principal);
	}

	private static Principal TestPrincipal() => new(new PrincipalId("principal-1"), "User One", "user@example.com");

	private static LocalSecretProtector TestSecretProtector() => new(1_000, 16, 32);
}
