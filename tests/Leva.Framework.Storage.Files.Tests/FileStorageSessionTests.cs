using Xunit;

namespace Leva.Framework.Storage.Files.Tests;

public sealed class FileStorageSessionTests
{
	[Fact]
	public async Task CommitAsync_MakesRepositoryChangesVisibleToExistingRootRepository()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
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
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
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
	public async Task CommitAsync_MakesJournalChangesVisibleToExistingRootJournal()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
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
	public async Task CommitAsync_AfterCompletedSessionReturnsFailure()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var session = ResultAssert.Success(await storage.OpenAsync());
		await session.CommitAsync();
		var secondCommit = await session.CommitAsync();

		Assert.True(secondCommit.IsFailure);
		Assert.Equal("storage.failed", secondCommit.Error.Code);
	}

	[Fact]
	public async Task CreateRepository_WithCompletedSessionThrows()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var session = ResultAssert.Success(await storage.OpenAsync());
		await session.RollbackAsync();

		Assert.Throws<InvalidOperationException>(() => storage.CreateRepository<string, string>("items", session));
	}

	[Fact]
	public async Task CreateRepository_WithSessionFromDifferentProviderThrows()
	{
		using var firstDirectory = new TestStorageDirectory();
		using var secondDirectory = new TestStorageDirectory();
		var firstStorage = CreateStorage(firstDirectory);
		var secondStorage = CreateStorage(secondDirectory);
		var session = ResultAssert.Success(await firstStorage.OpenAsync());

		Assert.Throws<ArgumentException>(() => secondStorage.CreateRepository<string, string>("items", session));
	}

	private static FileStorageProvider CreateStorage(TestStorageDirectory directory) =>
		new(new FileStorageOptions { RootPath = directory.Path });
}
