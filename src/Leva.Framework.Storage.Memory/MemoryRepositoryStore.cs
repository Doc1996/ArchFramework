using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Holds the mutable in-memory entries for one repository.
/// </summary>
internal sealed class MemoryRepositoryStore<TId, TValue>
	where TId : notnull
{
	private readonly Lock _lock = new();
	private readonly Dictionary<TId, StorageEntry<TValue>> _entries = new();
	private readonly string _name;

	internal MemoryRepositoryStore(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		_name = name;
	}

	internal Result<StorageEntry<TValue>> Save(TId id, TValue value, StorageVersion? expectedVersion)
	{
		var key = id.ToString() ?? string.Empty;
		lock (_lock)
		{
			var utcNow = DateTimeOffset.UtcNow;
			if (_entries.TryGetValue(id, out var existing))
			{
				if (expectedVersion.HasValue && existing.Version != expectedVersion.Value)
					return Result<StorageEntry<TValue>>.Fail(
						StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Version)
					);

				var updated = existing with { Value = value, Version = existing.Version.Next(), UpdatedAt = utcNow };
				_entries[id] = updated;
				return Result<StorageEntry<TValue>>.Ok(updated);
			}

			if (expectedVersion.HasValue)
				return Result<StorageEntry<TValue>>.Fail(StorageErrors.NotFound(_name, key));

			var created = new StorageEntry<TValue>(value, new StorageVersion(1), utcNow, utcNow);
			_entries[id] = created;
			return Result<StorageEntry<TValue>>.Ok(created);
		}
	}

	internal StorageEntry<TValue>? Load(TId id)
	{
		lock (_lock)
			return _entries.TryGetValue(id, out var entry) ? entry : null;
	}

	internal IReadOnlyDictionary<TId, StorageEntry<TValue>> LoadAll()
	{
		lock (_lock)
			return new Dictionary<TId, StorageEntry<TValue>>(_entries);
	}

	internal bool Exists(TId id)
	{
		lock (_lock)
			return _entries.ContainsKey(id);
	}

	internal Result Delete(TId id, StorageVersion? expectedVersion)
	{
		var key = id.ToString() ?? string.Empty;
		lock (_lock)
		{
			if (!_entries.TryGetValue(id, out var existing))
				return Result.Fail(StorageErrors.NotFound(_name, key));
			if (expectedVersion.HasValue && existing.Version != expectedVersion.Value)
				return Result.Fail(StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Version));

			_entries.Remove(id);
			return Result.Ok();
		}
	}
}
