using Leva.Framework.Fakes;
using Leva.Framework.Storage;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class StorageJournalContractTests
{
	[Theory]
	[InlineData("memory")]
	[InlineData("files")]
	[InlineData("sqlite")]
	public async Task JournalContract_AppendsEntriesWithMonotonicVersionsAndReadsInOrder(string providerName)
	{
		using var scope = StorageContractProviders.Create(providerName);
		var journal = scope.CreateJournal(JournalName(providerName));

		var first = ResultAssert.Success(await journal.AppendAsync(new StorageContractActivity("created", 1)));
		var second = ResultAssert.Success(await journal.AppendAsync(new StorageContractActivity("assigned", 2)));
		var third = ResultAssert.Success(await journal.AppendAsync(new StorageContractActivity("resolved", 3)));

		Assert.Equal(new StorageVersion(1), first.Version);
		Assert.Equal(new StorageVersion(2), second.Version);
		Assert.Equal(new StorageVersion(3), third.Version);
		Assert.Equal(first.CreatedAt, first.UpdatedAt);

		var all = ResultAssert.Success(await journal.ReadAsync());
		Assert.Equal(new[] { first, second, third }, all);
	}

	[Theory]
	[InlineData("memory")]
	[InlineData("files")]
	[InlineData("sqlite")]
	public async Task JournalContract_AfterVersionAndLimitFilterTheSameWayAcrossProviders(string providerName)
	{
		using var scope = StorageContractProviders.Create(providerName);
		var journal = scope.CreateJournal(JournalName(providerName));

		var first = ResultAssert.Success(await journal.AppendAsync(new StorageContractActivity("created", 1)));
		var second = ResultAssert.Success(await journal.AppendAsync(new StorageContractActivity("assigned", 2)));
		var third = ResultAssert.Success(await journal.AppendAsync(new StorageContractActivity("resolved", 3)));

		var afterFirst = ResultAssert.Success(await journal.ReadAsync(first.Version));
		var limited = ResultAssert.Success(await journal.ReadAsync(limit: 2));
		var afterFirstLimited = ResultAssert.Success(await journal.ReadAsync(first.Version, 1));
		var afterLatest = ResultAssert.Success(await journal.ReadAsync(third.Version));

		Assert.Equal(new[] { second, third }, afterFirst);
		Assert.Equal(new[] { first, second }, limited);
		Assert.Equal(new[] { second }, afterFirstLimited);
		Assert.Empty(afterLatest);
	}

	[Theory]
	[InlineData("memory")]
	[InlineData("files")]
	[InlineData("sqlite")]
	public async Task JournalContract_JournalNamesAreIsolated(string providerName)
	{
		using var scope = StorageContractProviders.Create(providerName);
		var firstJournal = scope.CreateJournal(JournalName(providerName, "first"));
		var secondJournal = scope.CreateJournal(JournalName(providerName, "second"));

		await firstJournal.AppendAsync(new StorageContractActivity("first", 1));
		await secondJournal.AppendAsync(new StorageContractActivity("second", 1));

		Assert.Equal("first", Assert.Single(ResultAssert.Success(await firstJournal.ReadAsync())).Value.Message);
		Assert.Equal("second", Assert.Single(ResultAssert.Success(await secondJournal.ReadAsync())).Value.Message);
	}

	private static string JournalName(string providerName, string suffix = "main") =>
		$"contract-{providerName}-{suffix}-{Guid.NewGuid():N}";
}
