namespace Leva.Framework.Identity.Local;

/// <summary>
/// Groups common local credential services and authentication policy.
/// </summary>
public sealed class LocalPrincipalServices
{
	private LocalPrincipalServices(
		ILocalCredentialStore credentials,
		LocalCredentialService credentialService,
		LocalAuthenticationPolicy authenticationPolicy
	)
	{
		Credentials = credentials;
		CredentialService = credentialService;
		AuthenticationPolicy = authenticationPolicy;
	}

	public ILocalCredentialStore Credentials { get; }
	public LocalCredentialService CredentialService { get; }
	public LocalAuthenticationPolicy AuthenticationPolicy { get; }

	public static LocalPrincipalServices Create(
		IPrincipalStore principals,
		ILocalCredentialStore? credentials = null,
		LocalSecretProtector? secretProtector = null
	)
	{
		ArgumentNullException.ThrowIfNull(principals);

		var localCredentials = credentials ?? new MemoryLocalCredentialStore();
		var localSecretProtector = secretProtector ?? new LocalSecretProtector();
		var credentialService = new LocalCredentialService(localCredentials, localSecretProtector);
		var authenticationPolicy = new LocalAuthenticationPolicy(localCredentials, principals, localSecretProtector);

		return new LocalPrincipalServices(localCredentials, credentialService, authenticationPolicy);
	}
}
