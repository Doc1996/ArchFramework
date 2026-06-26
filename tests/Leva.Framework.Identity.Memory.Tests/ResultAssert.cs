using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Identity.Memory.Tests;

internal static class ResultAssert
{
	public static T Success<T>(Result<T> result)
	{
		Assert.True(result.IsSuccess, result.Error.Message);
		return result.Value!;
	}

	public static void Success(Result result) => Assert.True(result.IsSuccess, result.Error.Message);
}
