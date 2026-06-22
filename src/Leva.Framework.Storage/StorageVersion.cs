namespace Leva.Framework.Storage;

/// <summary>
/// Represents a positive provider-assigned storage revision or journal sequence number.
/// </summary>
public readonly record struct StorageVersion
{
	public StorageVersion(long value)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
		Value = value;
	}

	public long Value { get; }

	public StorageVersion Next() => new(Value + 1);

	public override string ToString() => Value.ToString();
}
