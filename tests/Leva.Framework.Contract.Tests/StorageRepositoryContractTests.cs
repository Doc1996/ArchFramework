using Leva.Framework.Fakes;
using Leva.Framework.Storage;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class StorageRepositoryContractTests
{
	[Theory]
	[InlineData("memory")]
	[InlineData("files")]
	[InlineData("sqlite")]
	public async Task RepositoryContract_SaveLoadUpdateLoadAllExistsAndDelete_AreConsistent(string providerName)
	{
		using var scope = StorageContractProviders.Create(providerName);
		var repository = scope.CreateRepository(RepositoryName(providerName));

		var created = ResultAssert.Success(
			await repository.SaveAsync("item-1", new StorageContractDocument("first", 1))
		);
		var second = ResultAssert.Success(
			await repository.SaveAsync("item-2", new StorageContractDocument("second", 1))
		);

		Assert.Equal(new StorageVersion(1), created.Version);
		Assert.Equal(created.CreatedAt, created.UpdatedAt);
		Assert.True(ResultAssert.Success(await repository.ExistsAsync("item-1")));

		Assert.False(ResultAssert.Success(await repository.ExistsAsync("missing")));
		Assert.Equal(created, ResultAssert.Success(await repository.LoadAsync("item-1")));
		var all = ResultAssert.Success(await repository.LoadAllAsync());

		Assert.Equal(2, all.Count);
		Assert.Equal(created, all["item-1"]);
		Assert.Equal(second, all["item-2"]);

		var updated = ResultAssert.Success(
			await repository.SaveAsync("item-1", new StorageContractDocument("first-updated", 2), created.Version)
		);

		Assert.Equal(new StorageVersion(2), updated.Version);
		Assert.Equal(created.CreatedAt, updated.CreatedAt);
		Assert.True(updated.UpdatedAt >= updated.CreatedAt);
		Assert.Equal(updated, ResultAssert.Success(await repository.LoadAsync("item-1")));

		var staleUpdate = await repository.SaveAsync(
			"item-1",
			new StorageContractDocument("stale", 3),
			created.Version
		);
		Assert.True(staleUpdate.IsFailure);
		Assert.Equal("storage.version_conflict", staleUpdate.Error.Code);

		var staleDelete = await repository.DeleteAsync("item-1", created.Version);
		Assert.True(staleDelete.IsFailure);
		Assert.Equal("storage.version_conflict", staleDelete.Error.Code);

		ResultAssert.Success(await repository.DeleteAsync("item-1", updated.Version));
		Assert.False(ResultAssert.Success(await repository.ExistsAsync("item-1")));

		var missing = await repository.LoadAsync("item-1");
		Assert.True(missing.IsFailure);
		Assert.Equal("storage.not_found", missing.Error.Code);

		var remaining = ResultAssert.Success(await repository.LoadAllAsync());
		Assert.Single(remaining);
		Assert.Equal(second, remaining["item-2"]);
	}

	[Theory]
	[InlineData("memory")]
	[InlineData("files")]
	[InlineData("sqlite")]
	public async Task RepositoryContract_RepositoryNamesAreIsolated(string providerName)
	{
		using var scope = StorageContractProviders.Create(providerName);
		var firstRepository = scope.CreateRepository(RepositoryName(providerName, "first"));
		var secondRepository = scope.CreateRepository(RepositoryName(providerName, "second"));

		await firstRepository.SaveAsync("same-id", new StorageContractDocument("first", 1));
		await secondRepository.SaveAsync("same-id", new StorageContractDocument("second", 1));

		Assert.Equal("first", ResultAssert.Success(await firstRepository.LoadAsync("same-id")).Value.Name);
		Assert.Equal("second", ResultAssert.Success(await secondRepository.LoadAsync("same-id")).Value.Name);
	}

	[Theory]
	[InlineData("memory")]
	[InlineData("files")]
	[InlineData("sqlite")]
	public async Task RepositoryContract_ExpectedVersionOnMissingEntryReturnsNotFound(string providerName)
	{
		using var scope = StorageContractProviders.Create(providerName);
		var repository = scope.CreateRepository(RepositoryName(providerName));

		var save = await repository.SaveAsync(
			"missing",
			new StorageContractDocument("first", 1),
			new StorageVersion(1)
		);
		var delete = await repository.DeleteAsync("missing", new StorageVersion(1));

		Assert.True(save.IsFailure);
		Assert.Equal("storage.not_found", save.Error.Code);
		Assert.True(delete.IsFailure);
		Assert.Equal("storage.not_found", delete.Error.Code);
	}

	private static string RepositoryName(string providerName, string suffix = "main") =>
		$"contract-{providerName}-{suffix}-{Guid.NewGuid():N}";
}
