using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Authenticates principals by name through an in-memory principal store.
/// </summary>
public sealed class MemoryAuthenticationPolicy : AuthenticationPolicy
{
	private readonly IPrincipalStore _principals;

	public MemoryAuthenticationPolicy(IPrincipalStore principals, AuthenticationMethod? method = null)
	{
		ArgumentNullException.ThrowIfNull(principals);
		_principals = principals;
		Method = method ?? MemoryAuthenticationMethods.Principal;
	}

	public override AuthenticationMethod Method { get; }

	public override async Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		if (string.IsNullOrWhiteSpace(request.Name))
			return Succeeded(AuthenticationResult.Failed("Principal name is required."));

		var principal = await _principals.FindByNameAsync(request.Name, token);
		if (principal.IsFailure)
			return Result<AuthenticationResult>.Fail(principal.Error);

		return principal.Value is null
			? Succeeded(AuthenticationResult.Failed("Principal name is invalid."))
			: Succeeded(AuthenticationResult.Succeeded(principal.Value));
	}

	private static Result<AuthenticationResult> Succeeded(AuthenticationResult result) =>
		Result<AuthenticationResult>.Ok(result);
}
