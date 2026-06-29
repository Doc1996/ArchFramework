using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Identity.Local.Tests;

internal static class ResultAssert
{
	internal static T Success<T>(Result<T> result)
	{
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		return result.Value!;
	}
}
