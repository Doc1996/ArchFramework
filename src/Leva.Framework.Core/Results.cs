namespace Leva.Framework.Core;

/// <summary>
/// Represents success or failure without a returned value.
/// </summary>
public readonly record struct Result
{
	private Result(bool isSuccess, Error error)
	{
		IsSuccess = isSuccess;
		Error = error;
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	public Error Error { get; }

	public static Result Ok() => new(true, Error.None);

	public static Result Fail(Error error) => new(false, error);
}

/// <summary>
/// Represents success or failure with an optional returned value.
/// </summary>
public readonly record struct Result<T>
{
	private Result(bool isSuccess, T? value, Error error)
	{
		IsSuccess = isSuccess;
		Value = value;
		Error = error;
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	public T? Value { get; }
	public Error Error { get; }

	public static Result<T> Ok(T value) => new(true, value, Error.None);

	public static Result<T> Fail(Error error) => new(false, default, error);
}
