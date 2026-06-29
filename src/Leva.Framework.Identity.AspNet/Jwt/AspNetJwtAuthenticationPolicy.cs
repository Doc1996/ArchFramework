using Leva.Framework.Core;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Authenticates built in ASP.NET Core JWT access tokens.
/// </summary>
public sealed class AspNetJwtAuthenticationPolicy(IOptions<AspNetJwtOptions> options, AspNetJwtTokenService tokens)
	: AuthenticationPolicy
{
	private readonly AspNetJwtOptions _options = options.Value;
	public override AuthenticationMethod Method => _options.Method;

	public override Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		var validation = tokens.ValidateToken(request.Token ?? string.Empty);
		if (validation.IsFailure)
			return Task.FromResult(Result<AuthenticationResult>.Fail(validation.Error));

		var session = validation.Value!;
		return Task.FromResult(
			Result<AuthenticationResult>.Ok(AuthenticationResult.Succeeded(session.Principal).WithSession(session))
		);
	}
}
