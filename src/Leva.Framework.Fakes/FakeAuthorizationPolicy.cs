using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable authorization policy fake with call tracking.
/// </summary>
public sealed class FakeAuthorizationPolicy : AuthorizationPolicy
{
	public int AuthorizeCount { get; private set; }
	public AuthorizationRequest? LastRequest { get; private set; }
	public Func<AuthorizationRequest, bool> CanAuthorizeHandler { get; set; } = _ => true;
	public Result<AuthorizationResult>? Result { get; set; }

	public override bool CanAuthorize(AuthorizationRequest request) => CanAuthorizeHandler(request);

	public override Task<Result<AuthorizationResult>> AuthorizeAsync(
		AuthorizationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		AuthorizeCount++;
		LastRequest = request;

		return Task.FromResult(
			Result ?? Result<AuthorizationResult>.Ok(AuthorizationResult.Denied(request.Requirement, "Not configured."))
		);
	}
}
