namespace Leva.Framework.Identity;

/// <summary>
/// Audit sink implementation that intentionally ignores audit entries.
/// </summary>
public sealed class NullAuditSink : IAuditSink
{
	public void Write(AuditEntry auditEntry) { }

	public void Clear() { }
}
