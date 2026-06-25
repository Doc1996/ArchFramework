using Xunit;

namespace Leva.Framework.Storage.Sqlite.Tests;

public sealed class SqliteStorageSessionTests
{
	[Fact]
	public async Task CommitAsync_MakesRepositoryChangesVisibleToExistingRootRepository()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var rootRepository = storage.CreateRepository<string, string>("items");

		await rootRepository.SaveAsync("a", "root");
		var session = ResultAssert.Success(await storage.OpenAsync());
		var sessionRepository = storage.CreateRepository<string, string>("items", session);

		await sessionRepository.SaveAsync("a", "session", new StorageVersion(1));
		var commit = await session.CommitAsync();
		var loaded = ResultAssert.Success(await rootRepository.LoadAsync("a"));

		Assert.True(commit.IsSuccess);
		Assert.Equal("session", loaded.Value);
		Assert.Equal(new StorageVersion(2), loaded.Version);
	}

	[Fact]
	public async Task RollbackAsync_AbandonsRepositoryChanges()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var rootRepository = storage.CreateRepository<string, string>("items");

		await rootRepository.SaveAsync("a", "root");
		var session = ResultAssert.Success(await storage.OpenAsync());
		var sessionRepository = storage.CreateRepository<string, string>("items", session);

		await sessionRepository.SaveAsync("a", "session", new StorageVersion(1));
		var rollback = await session.RollbackAsync();
		var loaded = ResultAssert.Success(await rootRepository.LoadAsync("a"));

		Assert.True(rollback.IsSuccess);
		Assert.Equal("root", loaded.Value);
		Assert.Equal(new StorageVersion(1), loaded.Version);
	}

	[Fact]
	public async Task DisposeAsync_AbandonsRepositoryChanges()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var rootRepository = storage.CreateRepository<string, string>("items");

		await rootRepository.SaveAsync("a", "root");
		var session = ResultAssert.Success(await storage.OpenAsync());
		var sessionRepository = storage.CreateRepository<string, string>("items", session);

		await sessionRepository.SaveAsync("a", "session", new StorageVersion(1));
		await session.DisposeAsync();
		var loaded = ResultAssert.Success(await rootRepository.LoadAsync("a"));

		Assert.Equal("root", loaded.Value);
		Assert.Equal(new StorageVersion(1), loaded.Version);
	}

	[Fact]
	public async Task CommitAsync_MakesJournalChangesVisibleToExistingRootJournal()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var rootJournal = storage.CreateJournal<string>("events");

		await rootJournal.AppendAsync("root");
		var session = ResultAssert.Success(await storage.OpenAsync());
		var sessionJournal = storage.CreateJournal<string>("events", session);

		await sessionJournal.AppendAsync("session");
		var commit = await session.CommitAsync();
		var entries = ResultAssert.Success(await rootJournal.ReadAsync());

		Assert.True(commit.IsSuccess);
		Assert.Equal(["root", "session"], entries.Select(x => x.Value).ToArray());
	}

	[Fact]
	public async Task RollbackAsync_AbandonsJournalChanges()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var rootJournal = storage.CreateJournal<string>("events");

		await rootJournal.AppendAsync("root");
		var session = ResultAssert.Success(await storage.OpenAsync());
		var sessionJournal = storage.CreateJournal<string>("events", session);

		await sessionJournal.AppendAsync("session");
		var rollback = await session.RollbackAsync();
		var entries = ResultAssert.Success(await rootJournal.ReadAsync());

		Assert.True(rollback.IsSuccess);
		var entry = Assert.Single(entries);
		Assert.Equal("root", entry.Value);
	}

	[Fact]
	public async Task CommitAsync_AfterCompletedSessionReturnsFailure()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var session = ResultAssert.Success(await storage.OpenAsync());

		await session.CommitAsync();
		var secondCommit = await session.CommitAsync();

		Assert.True(secondCommit.IsFailure);
		Assert.Equal("storage.failed", secondCommit.Error.Code);
	}

	[Fact]
	public async Task CreateRepository_WithCompletedSessionThrows()
	{
		using var database = new TestSqliteDatabase();
		var storage = CreateStorage(database);
		var session = ResultAssert.Success(await storage.OpenAsync());

		await session.RollbackAsync();
		Assert.Throws<InvalidOperationException>(() => storage.CreateRepository<string, string>("items", session));
	}

	[Fact]
	public async Task CreateRepository_WithSessionFromDifferentProviderThrows()
	{
		using var firstDatabase = new TestSqliteDatabase();
		using var secondDatabase = new TestSqliteDatabase();
		var firstStorage = CreateStorage(firstDatabase);
		var secondStorage = CreateStorage(secondDatabase);

		var session = ResultAssert.Success(await firstStorage.OpenAsync());
		Assert.Throws<ArgumentException>(() => secondStorage.CreateRepository<string, string>("items", session));
	}

	private static SqliteStorageProvider CreateStorage(TestSqliteDatabase database) => new(database.Path);
}
