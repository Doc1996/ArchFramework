using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable authentication policy fake with call tracking.
/// </summary>
public sealed class FakeAuthenticationPolicy(AuthenticationMethod method) : AuthenticationPolicy
{
	public override AuthenticationMethod Method { get; } = method;
	public int AuthenticateCount { get; private set; }
	public AuthenticationRequest? LastRequest { get; private set; }

	public Result<AuthenticationResult> Result { get; set; } =
		Result<AuthenticationResult>.Ok(AuthenticationResult.Failed("Not configured."));

	public override Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		AuthenticateCount++;
		LastRequest = request;

		return Task.FromResult(Result);
	}
}
