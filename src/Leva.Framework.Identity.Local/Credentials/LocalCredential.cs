namespace Leva.Framework.Identity.Local;

/// <summary>
/// Connects a local login name and protected secret to a principal.
/// </summary>
public sealed record LocalCredential(
	string Name,
	PrincipalId PrincipalId,
	LocalCredentialSecret Secret,
	bool IsEnabled,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt
)
{
	public LocalCredential ChangeSecret(LocalCredentialSecret secret, DateTimeOffset utcNow) =>
		this with
		{
			Secret = secret,
			UpdatedAt = utcNow,
		};

	public LocalCredential Enable(DateTimeOffset utcNow) => this with { IsEnabled = true, UpdatedAt = utcNow };

	public LocalCredential Disable(DateTimeOffset utcNow) => this with { IsEnabled = false, UpdatedAt = utcNow };
}
