using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Stores diagnostic audit entries in memory for inspection, demos, or tests.
/// </summary>
public sealed class MemoryAuditSink : IAuditSink
{
	private readonly SyncList<AuditEntry> _auditEntries = new();
	public IReadOnlyList<AuditEntry> AuditEntries => _auditEntries.List();

	public void Write(AuditEntry auditEntry) => _auditEntries.Add(auditEntry);

	public void Clear() => _auditEntries.Clear();
}
