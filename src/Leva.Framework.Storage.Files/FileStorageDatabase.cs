using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Owns file paths, serialization, and named stores for one file storage provider instance.
/// </summary>
internal sealed class FileStorageDatabase
{
	private readonly Lock _lock = new();
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly Dictionary<string, object> _journals = new();
	private readonly Dictionary<string, object> _repositories = new();

	internal FileStorageDatabase(string rootPath, JsonSerializerOptions? jsonOptions = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
		RootPath = Path.GetFullPath(rootPath);
		_jsonOptions = jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
	}

	internal string RootPath { get; }

	internal FileRepositoryStore<TId, TValue> GetRepository<TId, TValue>(string name)
		where TId : notnull
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		var key = GetRepositoryKey<TId, TValue>(name);

		lock (_lock)
		{
			if (_repositories.TryGetValue(key, out var existing))
				return (FileRepositoryStore<TId, TValue>)existing;

			var store = new FileRepositoryStore<TId, TValue>(name, this, GetRepositoryDirectory<TId, TValue>(name));
			_repositories[key] = store;
			return store;
		}
	}

	internal FileJournalStore<TEntry> GetJournal<TEntry>(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		var key = GetJournalKey<TEntry>(name);

		lock (_lock)
		{
			if (_journals.TryGetValue(key, out var existing))
				return (FileJournalStore<TEntry>)existing;

			var store = new FileJournalStore<TEntry>(name, this, GetJournalDirectory<TEntry>(name));
			_journals[key] = store;
			return store;
		}
	}

	internal string GetRepositoryPath<TId, TValue>(string name, TId id)
		where TId : notnull => Path.Combine(GetRepositoryDirectory<TId, TValue>(name), Encode(ToKey(id)) + ".json");

	internal string GetJournalPath<TEntry>(string name, StorageVersion version) =>
		Path.Combine(
			GetJournalDirectory<TEntry>(name),
			version.Value.ToString("D20", CultureInfo.InvariantCulture) + ".json"
		);

	internal async Task<StorageEntry<T>> ReadEntryAsync<T>(string path, CancellationToken token)
	{
		await using var stream = File.OpenRead(path);
		var fileEntry =
			await JsonSerializer.DeserializeAsync<FileStorageEntry<T>>(stream, _jsonOptions, token)
			?? throw new InvalidOperationException("Storage file did not contain a valid entry.");

		return new StorageEntry<T>(
			fileEntry.Value,
			new StorageVersion(fileEntry.Version),
			fileEntry.CreatedAt,
			fileEntry.UpdatedAt
		);
	}

	internal async Task WriteEntryAsync<T>(string path, StorageEntry<T> entry, CancellationToken token)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
		var fileEntry = new FileStorageEntry<T>(entry.Value, entry.Version.Value, entry.CreatedAt, entry.UpdatedAt);

		try
		{
			await using (var stream = File.Create(temporaryPath))
				await JsonSerializer.SerializeAsync(stream, fileEntry, _jsonOptions, token);
			File.Move(temporaryPath, path, true);
		}
		finally
		{
			if (File.Exists(temporaryPath))
				File.Delete(temporaryPath);
		}
	}

	internal static string GetFileKey(string path) => Decode(Path.GetFileNameWithoutExtension(path));

	internal static string ToKey<TId>(TId id)
		where TId : notnull
	{
		if (id is string value)
			return value;

		var converter = TypeDescriptor.GetConverter(typeof(TId));
		if (converter.CanConvertTo(typeof(string)))
			return converter.ConvertToInvariantString(id)
				?? throw new InvalidOperationException($"Could not convert '{typeof(TId).Name}' to a storage key.");

		return id.ToString()
			?? throw new InvalidOperationException($"Could not convert '{typeof(TId).Name}' to a storage key.");
	}

	internal static TId FromKey<TId>(string key)
		where TId : notnull
	{
		if (typeof(TId) == typeof(string))
			return (TId)(object)key;

		var converter = TypeDescriptor.GetConverter(typeof(TId));
		if (converter.CanConvertFrom(typeof(string)))
			return (TId)(
				converter.ConvertFromInvariantString(key)
				?? throw new InvalidOperationException($"Could not convert storage key to '{typeof(TId).Name}'.")
			);

		return (TId)Convert.ChangeType(key, typeof(TId), CultureInfo.InvariantCulture);
	}

	internal static StorageVersion ParseVersion(string path) =>
		new(long.Parse(Path.GetFileNameWithoutExtension(path), CultureInfo.InvariantCulture));

	private string GetRepositoryDirectory<TId, TValue>(string name)
		where TId : notnull => Path.Combine(RootPath, "repositories", Hash(GetRepositoryKey<TId, TValue>(name)));

	private string GetJournalDirectory<TEntry>(string name) =>
		Path.Combine(RootPath, "journals", Hash(GetJournalKey<TEntry>(name)));

	private static string GetRepositoryKey<TId, TValue>(string name) =>
		$"{name}|{GetTypeKey<TId>()}|{GetTypeKey<TValue>()}";

	private static string GetJournalKey<TEntry>(string name) => $"{name}|{GetTypeKey<TEntry>()}";

	private static string GetTypeKey<T>() => typeof(T).FullName ?? typeof(T).Name;

	private static string Hash(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
	}

	private static string Encode(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
	}

	private static string Decode(string value)
	{
		var base64 = value.Replace('-', '+').Replace('_', '/');
		var padding = (4 - base64.Length % 4) % 4;
		base64 = base64.PadRight(base64.Length + padding, '=');

		return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
	}
}
