using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Holds file operations for one named append-only journal folder.
/// </summary>
internal sealed class FileJournalStore<TEntry>
{
	private readonly string _name;
	private readonly FileStorageDatabase _database;
	private readonly string _directory;
	private readonly FileStorageRunner _runner;

	public FileJournalStore(string name, FileStorageDatabase database, string directory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);

		_name = name;
		_database = database;
		_directory = directory;
		_runner = new($"file journal '{name}'");
	}

	internal Task<Result<StorageEntry<TEntry>>> AppendAsync(TEntry value, CancellationToken token) =>
		_runner.RunAsync(
			"append",
			async () =>
			{
				Directory.CreateDirectory(_directory);

				var version = GetNextVersion();
				var utcNow = DateTimeOffset.UtcNow;
				var entry = new StorageEntry<TEntry>(value, version, utcNow, utcNow);
				var path = _database.GetJournalPath<TEntry>(_name, version);

				await _database.WriteEntryAsync(path, entry, token);
				return Result<StorageEntry<TEntry>>.Ok(entry);
			},
			token
		);

	internal Task<Result<IReadOnlyList<StorageEntry<TEntry>>>> ReadAsync(
		StorageVersion? afterVersion,
		int? limit,
		CancellationToken token
	) =>
		_runner.RunAsync(
			"read",
			async () =>
			{
				var entries = new List<StorageEntry<TEntry>>();
				if (!Directory.Exists(_directory))
					return Result<IReadOnlyList<StorageEntry<TEntry>>>.Ok(entries);

				foreach (var file in GetFiles(afterVersion, limit))
				{
					token.ThrowIfCancellationRequested();
					entries.Add(await _database.ReadEntryAsync<TEntry>(file.Path, token));
				}

				return Result<IReadOnlyList<StorageEntry<TEntry>>>.Ok(entries);
			},
			token
		);

	private IEnumerable<(string Path, StorageVersion Version)> GetFiles(StorageVersion? afterVersion, int? limit)
	{
		var files = Directory
			.EnumerateFiles(_directory, "*.json")
			.Select(path => (Path: path, Version: FileStorageDatabase.ParseVersion(path)))
			.Where(file => !afterVersion.HasValue || file.Version.Value > afterVersion.Value.Value)
			.OrderBy(file => file.Version.Value);

		return limit.HasValue ? files.Take(limit.Value) : files;
	}

	private StorageVersion GetNextVersion()
	{
		var latest = Directory
			.EnumerateFiles(_directory, "*.json")
			.Select(FileStorageDatabase.ParseVersion)
			.Select(version => version.Value)
			.DefaultIfEmpty(0)
			.Max();

		return new StorageVersion(latest + 1);
	}
}
