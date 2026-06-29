using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Storage.Tests;

public sealed class StorageContractTests
{
	[Fact]
	public async Task RepositoryContract_CanBeImplemented()
	{
		IRepository<string, string> repository = new TestRepository();
		var saved = await repository.SaveAsync("id", "value");
		var loaded = await repository.LoadAsync("id");

		Assert.True(saved.IsSuccess);
		var loadedEntry = ResultAssert.Success(loaded);
		Assert.Equal("value", loadedEntry.Value);
	}

	[Fact]
	public async Task JournalContract_CanBeImplemented()
	{
		IJournal<string> journal = new TestJournal();

		var appended = await journal.AppendAsync("entry");
		var entries = await journal.ReadAsync();
		var storedEntries = ResultAssert.Success(entries);

		Assert.True(appended.IsSuccess);
		Assert.Single(storedEntries);
	}

	private sealed class TestRepository : IRepository<string, string>
	{
		private StorageEntry<string>? _entry;

		public Task<Result<StorageEntry<string>>> SaveAsync(
			string id,
			string value,
			StorageVersion? expectedVersion = null,
			CancellationToken token = default
		)
		{
			_entry = new StorageEntry<string>(
				value,
				new StorageVersion(1),
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow
			);
			return Task.FromResult(Result<StorageEntry<string>>.Ok(_entry.Value));
		}

		public Task<Result<StorageEntry<string>>> LoadAsync(string id, CancellationToken token = default)
		{
			return Task.FromResult(
				_entry.HasValue
					? Result<StorageEntry<string>>.Ok(_entry.Value)
					: Result<StorageEntry<string>>.Fail(StorageErrors.NotFound("test", id))
			);
		}

		public Task<Result<IReadOnlyDictionary<string, StorageEntry<string>>>> LoadAllAsync(
			CancellationToken token = default
		)
		{
			return Task.FromResult(
				Result<IReadOnlyDictionary<string, StorageEntry<string>>>.Ok(
					new Dictionary<string, StorageEntry<string>>()
				)
			);
		}

		public Task<Result<bool>> ExistsAsync(string id, CancellationToken token = default)
		{
			return Task.FromResult(Result<bool>.Ok(_entry.HasValue));
		}

		public Task<Result> DeleteAsync(
			string id,
			StorageVersion? expectedVersion = null,
			CancellationToken token = default
		)
		{
			return Task.FromResult(Result.Ok());
		}
	}

	private sealed class TestJournal : IJournal<string>
	{
		private readonly List<StorageEntry<string>> _entries = [];

		public Task<Result<StorageEntry<string>>> AppendAsync(string entry, CancellationToken token = default)
		{
			var stored = new StorageEntry<string>(
				entry,
				new StorageVersion(1),
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow
			);

			_entries.Add(stored);
			return Task.FromResult(Result<StorageEntry<string>>.Ok(stored));
		}

		public Task<Result<IReadOnlyList<StorageEntry<string>>>> ReadAsync(
			StorageVersion? afterVersion = null,
			int? limit = null,
			CancellationToken token = default
		)
		{
			return Task.FromResult(Result<IReadOnlyList<StorageEntry<string>>>.Ok(_entries));
		}
	}
}
