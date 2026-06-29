namespace Leva.Framework.Identity.Local;

/// <summary>
/// Groups common local credential services and authentication policy.
/// </summary>
public sealed class LocalPrincipalServices(
	ILocalCredentialStore credentials,
	ILocalSecretProtector secretProtector,
	LocalCredentialService credentialService,
	LocalAuthenticationPolicy authenticationPolicy
)
{
	public ILocalCredentialStore Credentials { get; } = credentials;
	public ILocalSecretProtector SecretProtector { get; } = secretProtector;
	public LocalCredentialService CredentialService { get; } = credentialService;
	public LocalAuthenticationPolicy AuthenticationPolicy { get; } = authenticationPolicy;

	public static LocalPrincipalServices Create(
		IPrincipalStore principals,
		ILocalCredentialStore? credentials = null,
		ILocalSecretProtector? secretProtector = null
	)
	{
		ArgumentNullException.ThrowIfNull(principals);

		var localCredentials = credentials ?? new MemoryLocalCredentialStore();
		var localSecretProtector = secretProtector ?? new LocalSecretProtector();
		var credentialService = new LocalCredentialService(localCredentials, localSecretProtector);
		var authenticationPolicy = new LocalAuthenticationPolicy(localCredentials, principals, localSecretProtector);

		return new LocalPrincipalServices(
			localCredentials,
			localSecretProtector,
			credentialService,
			authenticationPolicy
		);
	}
}
