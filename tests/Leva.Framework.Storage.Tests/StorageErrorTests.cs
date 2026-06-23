using Xunit;

namespace Leva.Framework.Storage.Tests;

public sealed class StorageErrorTests
{
	[Fact]
	public void NotFound_CreatesStructuredError()
	{
		var error = StorageErrors.NotFound("plans", "42");

		Assert.Equal("storage.not_found", error.Code);
		Assert.Contains("plans", error.Message);
		Assert.Contains("42", error.Message);
	}

	[Fact]
	public void VersionConflict_CreatesStructuredError()
	{
		var error = StorageErrors.VersionConflict("plans", "42", new StorageVersion(2), new StorageVersion(3));

		Assert.Equal("storage.version_conflict", error.Code);
		Assert.Contains("2", error.Message);
		Assert.Contains("3", error.Message);
	}

	[Fact]
	public void Failed_AppendsDetailsWhenProvided()
	{
		var error = StorageErrors.Failed("save", "disk full");

		Assert.Equal("storage.failed", error.Code);
		Assert.Contains("save", error.Message);
		Assert.Contains("disk full", error.Message);
	}
}
