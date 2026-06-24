using Xunit;

namespace Leva.Framework.Storage.Files.Tests;

public sealed class FileRepositoryTests
{
	[Fact]
	public async Task SaveAsync_CreatesNewEntryWithVersionOne()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		var result = ResultAssert.Success(await repository.SaveAsync("a", "first"));

		Assert.Equal("first", result.Value);
		Assert.Equal(new StorageVersion(1), result.Version);
		Assert.Equal(result.CreatedAt, result.UpdatedAt);
	}

	[Fact]
	public async Task SaveAsync_UpdatesExistingEntryAndIncrementsVersion()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		var created = ResultAssert.Success(await repository.SaveAsync("a", "first"));
		var updated = ResultAssert.Success(await repository.SaveAsync("a", "second", created.Version));

		Assert.Equal("second", updated.Value);
		Assert.Equal(new StorageVersion(2), updated.Version);
		Assert.Equal(created.CreatedAt, updated.CreatedAt);
		Assert.True(updated.UpdatedAt >= updated.CreatedAt);
	}

	[Fact]
	public async Task SaveAsync_WithWrongExpectedVersionReturnsConflict()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		await repository.SaveAsync("a", "first");

		var result = await repository.SaveAsync("a", "second", new StorageVersion(99));
		Assert.True(result.IsFailure);
		Assert.Equal("storage.version_conflict", result.Error.Code);
	}

	[Fact]
	public async Task LoadAsync_ReturnsStoredEntryAfterProviderRecreation()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		await repository.SaveAsync("a", "first");

		var recreatedStorage = CreateStorage(directory);
		var recreatedRepository = recreatedStorage.CreateRepository<string, string>("items");
		var entry = ResultAssert.Success(await recreatedRepository.LoadAsync("a"));

		Assert.Equal("first", entry.Value);
	}

	[Fact]
	public async Task LoadAsync_WhenMissingReturnsNotFound()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		var result = await repository.LoadAsync("missing");

		Assert.True(result.IsFailure);
		Assert.Equal("storage.not_found", result.Error.Code);
	}

	[Fact]
	public async Task LoadAllAsync_ReturnsAllStoredEntries()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");

		await repository.SaveAsync("a", "first");
		await repository.SaveAsync("b", "second");
		var entries = ResultAssert.Success(await repository.LoadAllAsync());

		Assert.Equal(2, entries.Count);
		Assert.Equal("first", entries["a"].Value);
		Assert.Equal("second", entries["b"].Value);
	}

	[Fact]
	public async Task ExistsAsync_ReturnsTrueOnlyForStoredEntry()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		await repository.SaveAsync("a", "first");

		var existing = ResultAssert.Success(await repository.ExistsAsync("a"));
		var missing = ResultAssert.Success(await repository.ExistsAsync("missing"));

		Assert.True(existing);
		Assert.False(missing);
	}

	[Fact]
	public async Task DeleteAsync_RemovesStoredEntry()
	{
		using var directory = new TestStorageDirectory();
		var storage = CreateStorage(directory);
		var repository = storage.CreateRepository<string, string>("items");
		var saved = ResultAssert.Success(await repository.SaveAsync("a", "first"));

		var deleted = await repository.DeleteAsync("a", saved.Version);
		var loaded = await repository.LoadAsync("a");

		Assert.True(deleted.IsSuccess);
		Assert.True(loaded.IsFailure);
	}

	private static FileStorageProvider CreateStorage(TestStorageDirectory directory) => new(directory.Path);
}
