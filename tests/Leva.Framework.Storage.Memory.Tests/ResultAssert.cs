using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Storage.Memory.Tests;

internal static class ResultAssert
{
	internal static T Success<T>(Result<T> result)
	{
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		return result.Value!;
	}
}
