namespace Leva.Framework.Identity;

/// <summary>
/// Receives audit entries for storage, diagnostics, tests, or host-defined processing.
/// </summary>
public interface IAuditSink
{
	void Write(AuditEntry auditEntry);
	void Clear();
}
