using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Core.Tests;

public sealed class ResultTests
{
	[Fact]
	public void ResultOk_IsSuccess()
	{
		var result = Result.Ok();

		Assert.True(result.IsSuccess);
		Assert.False(result.IsFailure);
		Assert.Equal(Error.None, result.Error);
	}

	[Fact]
	public void ResultFail_HasError()
	{
		var error = new Error("Invalid", "Invalid value.");
		var result = Result.Fail(error);

		Assert.False(result.IsSuccess);
		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void ResultOfTOk_HasValue()
	{
		var result = Result<int>.Ok(42);

		Assert.True(result.IsSuccess);
		Assert.Equal(42, result.Value);
	}

	[Fact]
	public void ResultOfTFail_HasNoValue()
	{
		var error = new Error("Missing", "Value missing.");
		var result = Result<int>.Fail(error);

		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
	}
}
