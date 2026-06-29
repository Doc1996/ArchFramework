namespace Leva.Framework.Identity;

/// <summary>
/// Records one security-relevant principal action without storing secrets.
/// </summary>
public sealed record AuditEntry(
	AuditAction Action,
	DateTimeOffset CreatedAt,
	PrincipalId? PrincipalId = null,
	AuthSessionId? SessionId = null,
	string? Method = null,
	string? Requirement = null,
	string? Reason = null,
	IReadOnlyDictionary<string, object?>? Properties = null
);
