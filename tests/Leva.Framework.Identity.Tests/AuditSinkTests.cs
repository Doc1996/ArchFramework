using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class AuditSinkTests
{
	[Fact]
	public void MemoryAuditSink_StoresEntriesAndCanClear()
	{
		var sink = new MemoryAuditSink();
		var entry = new AuditEntry(AuditAction.Authenticated, DateTimeOffset.UtcNow);

		sink.Write(entry);
		Assert.Single(sink.AuditEntries);
		Assert.Equal(entry, sink.AuditEntries[0]);

		sink.Clear();
		Assert.Empty(sink.AuditEntries);
	}

	[Fact]
	public void NullAuditSink_IgnoresEntries()
	{
		var sink = new NullAuditSink();
		sink.Write(new AuditEntry(AuditAction.AuthenticationFailed, DateTimeOffset.UtcNow));
	}
}
