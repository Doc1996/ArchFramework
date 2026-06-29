using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Testing;

/// <summary>
/// Shared assertions for framework tests that return Result values.
/// </summary>
public static class ResultAssert
{
	public static void Success(Result result) => Assert.True(result.IsSuccess, result.Error.Message);

	public static TValue Success<TValue>(Result<TValue> result)
	{
		Assert.True(result.IsSuccess, result.Error.Message);
		return result.Value!;
	}
}
