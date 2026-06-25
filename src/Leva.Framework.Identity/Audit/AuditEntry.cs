namespace Leva.Framework.Identity;

/// <summary>
/// Records one security-relevant identity action without storing secrets.
/// </summary>
public sealed record AuditEntry(
	AuditAction Action,
	DateTimeOffset CreatedAt,
	IdentityId? IdentityId = null,
	IdentitySessionId? SessionId = null,
	string? Method = null,
	string? Requirement = null,
	string? Reason = null,
	IReadOnlyDictionary<string, object?>? Properties = null
);
