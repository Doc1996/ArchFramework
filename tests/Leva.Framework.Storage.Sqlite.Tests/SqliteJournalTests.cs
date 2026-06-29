using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Storage.Sqlite.Tests;

public sealed class SqliteJournalTests
{
	[Fact]
	public async Task AppendAsync_AssignsIncreasingVersions()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var journal = storage.CreateJournal<string>("events");

		var first = ResultAssert.Success(await journal.AppendAsync("first"));
		var second = ResultAssert.Success(await journal.AppendAsync("second"));

		Assert.Equal(new StorageVersion(1), first.Version);
		Assert.Equal(new StorageVersion(2), second.Version);
	}

	[Fact]
	public async Task ReadAsync_ReturnsEntriesInAppendOrderAfterProviderRecreation()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var journal = storage.CreateJournal<string>("events");

		await journal.AppendAsync("first");
		await journal.AppendAsync("second");

		var recreatedStorage = CreateStorage(database);
		var recreatedJournal = recreatedStorage.CreateJournal<string>("events");
		var entries = ResultAssert.Success(await recreatedJournal.ReadAsync());

		Assert.Collection(
			entries,
			entry => Assert.Equal("first", entry.Value),
			entry => Assert.Equal("second", entry.Value)
		);
	}

	[Fact]
	public async Task ReadAsync_CanReadAfterVersion()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
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
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
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
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var journal = storage.CreateJournal<string>("events");

		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => journal.ReadAsync(limit: 0));
	}

	private static SqliteStorageProvider CreateStorage(TestSqliteDatabase database) => new(database.Path);
}
