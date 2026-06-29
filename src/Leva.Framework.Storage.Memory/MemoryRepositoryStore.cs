using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Holds the mutable in-memory entries for one repository.
/// </summary>
internal sealed class MemoryRepositoryStore<TId, TModel>
	where TId : notnull
{
	private readonly Lock _lock = new();
	private readonly Dictionary<TId, StorageEntry<TModel>> _entries = new();
	private readonly string _name;

	internal MemoryRepositoryStore(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		_name = name;
	}

	public Result<StorageEntry<TModel>> Save(TId id, TModel model, StorageVersion? expectedVersion)
	{
		var key = id.ToString() ?? string.Empty;
		lock (_lock)
		{
			var utcNow = DateTimeOffset.UtcNow;
			if (_entries.TryGetValue(id, out var existing))
			{
				if (expectedVersion.HasValue && existing.Version != expectedVersion.Value)
					return Result<StorageEntry<TModel>>.Fail(
						StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Version)
					);

				var updated = existing with { Value = model, Version = existing.Version.Next(), UpdatedAt = utcNow };
				_entries[id] = updated;
				return Result<StorageEntry<TModel>>.Ok(updated);
			}

			if (expectedVersion.HasValue)
				return Result<StorageEntry<TModel>>.Fail(StorageErrors.NotFound(_name, key));

			var created = new StorageEntry<TModel>(model, new StorageVersion(1), utcNow, utcNow);
			_entries[id] = created;
			return Result<StorageEntry<TModel>>.Ok(created);
		}
	}

	public StorageEntry<TModel>? Load(TId id)
	{
		lock (_lock)
			return _entries.TryGetValue(id, out var entry) ? entry : null;
	}

	public IReadOnlyDictionary<TId, StorageEntry<TModel>> LoadAll()
	{
		lock (_lock)
			return new Dictionary<TId, StorageEntry<TModel>>(_entries);
	}

	public bool Exists(TId id)
	{
		lock (_lock)
			return _entries.ContainsKey(id);
	}

	public Result Delete(TId id, StorageVersion? expectedVersion)
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
