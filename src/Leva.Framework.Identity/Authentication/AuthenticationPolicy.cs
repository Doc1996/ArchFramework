using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Authenticates requests for one method-specific provider.
/// </summary>
public abstract class AuthenticationPolicy
{
	public abstract AuthenticationMethod Method { get; }

	public virtual bool CanAuthenticate(AuthenticationRequest request) => request.Method == Method;

	public abstract Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	);
}
