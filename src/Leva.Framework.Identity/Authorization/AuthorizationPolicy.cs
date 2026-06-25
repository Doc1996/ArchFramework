using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Evaluates whether a principal satisfies an authorization requirement.
/// </summary>
public abstract class AuthorizationPolicy
{
	public virtual bool CanAuthorize(AuthorizationRequest request) => true;

	public abstract Task<Result<AuthorizationResult>> AuthorizeAsync(
		AuthorizationRequest request,
		CancellationToken token = default
	);
}
