namespace Leva.Framework.Identity;

/// <summary>
/// Represents the outcome of an authentication attempt and optional created session.
/// </summary>
public sealed record AuthenticationResult(
	bool IsAuthenticated,
	Identity? Identity = null,
	IdentitySession? Session = null,
	string? Reason = null
)
{
	public static AuthenticationResult Succeeded(Identity identity) => new(true, identity);

	public static AuthenticationResult Failed(string reason) => new(false, Reason: reason);

	public AuthenticationResult WithSession(IdentitySession session) => this with { Session = session };
}
