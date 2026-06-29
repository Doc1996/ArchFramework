namespace Leva.Framework.Identity;

/// <summary>
/// Represents the outcome of an authentication attempt and optional created session.
/// </summary>
public sealed record AuthenticationResult(
	bool IsAuthenticated,
	Principal? Principal = null,
	AuthSession? Session = null,
	string? Reason = null
)
{
	public static AuthenticationResult Succeeded(Principal principal) => new(true, principal);

	public static AuthenticationResult Failed(string reason) => new(false, Reason: reason);

	public AuthenticationResult WithSession(AuthSession session) => this with { Session = session };
}
