using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Owns file paths and serialization for one file storage provider instance.
/// </summary>
internal sealed class FileStorageDatabase
{
	private readonly JsonSerializerOptions _jsonOptions;

	public FileStorageDatabase(string rootPath, JsonSerializerOptions? jsonOptions = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
		RootPath = Path.GetFullPath(rootPath);
		_jsonOptions = jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
	}

	internal string RootPath { get; }

	public FileRepositoryStore<TId, TValue> GetRepository<TId, TValue>(string name)
		where TId : notnull => new(name, this, GetRepositoryDirectory<TId, TValue>(name));

	public FileJournalStore<TEntry> GetJournal<TEntry>(string name) =>
		new(name, this, GetJournalDirectory<TEntry>(name));

	public string GetRepositoryPath<TId, TValue>(string name, TId id)
		where TId : notnull => Path.Combine(GetRepositoryDirectory<TId, TValue>(name), Encode(ToKey(id)) + ".json");

	public string GetJournalPath<TEntry>(string name, StorageVersion version) =>
		Path.Combine(
			GetJournalDirectory<TEntry>(name),
			version.Value.ToString("D20", CultureInfo.InvariantCulture) + ".json"
		);

	public static string GetFileKey(string path) => Decode(Path.GetFileNameWithoutExtension(path));

	public static string ToKey<TId>(TId id)
		where TId : notnull
	{
		if (id is string value)
			return value;

		var converter = TypeDescriptor.GetConverter(typeof(TId));
		if (converter.CanConvertTo(typeof(string)))
			return converter.ConvertToInvariantString(id) ?? id.ToString() ?? string.Empty;

		return id.ToString() ?? string.Empty;
	}

	public static TId FromKey<TId>(string key)
		where TId : notnull
	{
		if (typeof(TId) == typeof(string))
			return (TId)(object)key;

		var converter = TypeDescriptor.GetConverter(typeof(TId));
		if (converter.CanConvertFrom(typeof(string)))
			return (TId)converter.ConvertFromInvariantString(key)!;

		return (TId)Convert.ChangeType(key, typeof(TId), CultureInfo.InvariantCulture);
	}

	public async Task<StorageEntry<T>> ReadEntryAsync<T>(string path, CancellationToken token)
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

	public async Task WriteEntryAsync<T>(string path, StorageEntry<T> entry, CancellationToken token)
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

	public static StorageVersion ParseVersion(string path) =>
		new(long.Parse(Path.GetFileNameWithoutExtension(path), CultureInfo.InvariantCulture));

	private string GetRepositoryDirectory<TId, TValue>(string name)
		where TId : notnull
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Path.Combine(RootPath, "repositories", Hash(GetRepositoryKey<TId, TValue>(name)));
	}

	private string GetJournalDirectory<TEntry>(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Path.Combine(RootPath, "journals", Hash(GetJournalKey<TEntry>(name)));
	}

	private static string GetRepositoryKey<TId, TValue>(string name) =>
		$"{name}|{typeof(TId).AssemblyQualifiedName}|{typeof(TValue).AssemblyQualifiedName}";

	private static string GetJournalKey<TEntry>(string name) => $"{name}|{typeof(TEntry).AssemblyQualifiedName}";

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
