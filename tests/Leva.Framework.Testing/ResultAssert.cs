using Leva.Framework.Core;

namespace Leva.Framework.Testing;

/// <summary>
/// Shared assertions for framework tests that return Result values.
/// </summary>
public static class ResultAssert
{
	public static void Success(Result result)
	{
		if (result.IsFailure)
			throw new ResultAssertionException(result.Error.Message);
	}

	public static TValue Success<TValue>(Result<TValue> result)
	{
		if (result.IsFailure)
			throw new ResultAssertionException(result.Error.Message);

		return result.Value!;
	}
}
