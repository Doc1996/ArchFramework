using Xunit;

namespace Leva.Framework.Storage.Memory.Tests;

public sealed class MemoryJournalTests
{
	[Fact]
	public async Task AppendAsync_AssignsIncreasingVersions()
	{
		var storage = new MemoryStorageProvider();
		var journal = storage.CreateJournal<string>("events");
		var first = ResultAssert.Success(await journal.AppendAsync("first"));
		var second = ResultAssert.Success(await journal.AppendAsync("second"));

		Assert.Equal(new StorageVersion(1), first.Version);
		Assert.Equal(new StorageVersion(2), second.Version);
	}

	[Fact]
	public async Task ReadAsync_ReturnsEntriesInAppendOrder()
	{
		var storage = new MemoryStorageProvider();
		var journal = storage.CreateJournal<string>("events");

		await journal.AppendAsync("first");
		await journal.AppendAsync("second");
		var entries = ResultAssert.Success(await journal.ReadAsync());

		Assert.Collection(
			entries,
			entry => Assert.Equal("first", entry.Value),
			entry => Assert.Equal("second", entry.Value)
		);
	}

	[Fact]
	public async Task ReadAsync_CanReadAfterVersion()
	{
		var storage = new MemoryStorageProvider();
		var journal = storage.CreateJournal<string>("events");

		await journal.AppendAsync("first");
		await journal.AppendAsync("second");
		await journal.AppendAsync("third");

		var entries = ResultAssert.Success(await journal.ReadAsync(afterVersion: new StorageVersion(1)));
		Assert.Equal(["second", "third"], entries.Select(x => x.Value).ToArray());
	}

	[Fact]
	public async Task ReadAsync_CanLimitReturnedEntries()
	{
		var storage = new MemoryStorageProvider();
		var journal = storage.CreateJournal<string>("events");
		await journal.AppendAsync("first");
		await journal.AppendAsync("second");

		var entries = ResultAssert.Success(await journal.ReadAsync(limit: 1));
		var entry = Assert.Single(entries);
		Assert.Equal("first", entry.Value);
	}

	[Fact]
	public async Task ReadAsync_RejectsInvalidLimit()
	{
		var storage = new MemoryStorageProvider();
		var journal = storage.CreateJournal<string>("events");
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => journal.ReadAsync(limit: 0));
	}
}
