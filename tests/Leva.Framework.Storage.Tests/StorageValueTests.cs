using Xunit;

namespace Leva.Framework.Storage.Tests;

public sealed class StorageValueTests
{
	[Fact]
	public void StorageVersion_StoresPositiveValue()
	{
		var version = new StorageVersion(3);
		Assert.Equal(3, version.Value);
		Assert.Equal("3", version.ToString());
	}

	[Fact]
	public void StorageVersion_RejectsZeroValue()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new StorageVersion(0));
	}

	[Fact]
	public void StorageVersion_NextIncrementsValue()
	{
		var version = new StorageVersion(7);
		Assert.Equal(new StorageVersion(8), version.Next());
	}

	[Fact]
	public void StorageEntry_StoresValueAndMetadata()
	{
		var createdAt = DateTimeOffset.UtcNow;
		var updatedAt = createdAt.AddSeconds(1);
		var entry = new StorageEntry<string>("value", new StorageVersion(1), createdAt, updatedAt);

		Assert.Equal("value", entry.Value);
		Assert.Equal(new StorageVersion(1), entry.Version);

		Assert.Equal(createdAt, entry.CreatedAt);
		Assert.Equal(updatedAt, entry.UpdatedAt);
	}
}
