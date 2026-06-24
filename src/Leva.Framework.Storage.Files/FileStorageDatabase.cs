using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Owns file paths, serialization, and directory copying for one file storage provider instance.
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

	public FileStorageDatabase Clone()
	{
		var database = new FileStorageDatabase(
			Path.Combine(Path.GetTempPath(), "leva-storage-files-" + Guid.NewGuid().ToString("N")),
			_jsonOptions
		);

		CopyDirectory(RootPath, database.RootPath);
		return database;
	}

	public void ReplaceWith(FileStorageDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);

		if (Directory.Exists(RootPath))
			Directory.Delete(RootPath, true);

		CopyDirectory(database.RootPath, RootPath);
	}

	public void Delete()
	{
		if (Directory.Exists(RootPath))
			Directory.Delete(RootPath, true);
	}

	public FileRepositoryStore<TId, TModel> GetRepository<TId, TModel>(string name)
		where TId : notnull => new(name, this, GetRepositoryDirectory<TId, TModel>(name));

	public FileJournalStore<TEntry> GetJournal<TEntry>(string name) =>
		new(name, this, GetJournalDirectory<TEntry>(name));

	public string GetRepositoryPath<TId, TModel>(string name, TId id)
		where TId : notnull => Path.Combine(GetRepositoryDirectory<TId, TModel>(name), Encode(ToKey(id)) + ".json");

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
		var record =
			await JsonSerializer.DeserializeAsync<FileStorageRecord<T>>(stream, _jsonOptions, token)
			?? throw new InvalidOperationException("Storage file did not contain a valid entry.");

		return new StorageEntry<T>(
			record.Value,
			new StorageVersion(record.Version),
			record.CreatedAt,
			record.UpdatedAt,
			record.Properties
		);
	}

	public async Task WriteEntryAsync<T>(string path, StorageEntry<T> entry, CancellationToken token)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";

		try
		{
			var record = new FileStorageRecord<T>(
				entry.Value,
				entry.Version.Value,
				entry.CreatedAt,
				entry.UpdatedAt,
				entry.Properties
			);

			await using (var stream = File.Create(temporaryPath))
				await JsonSerializer.SerializeAsync(stream, record, _jsonOptions, token);

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

	private string GetRepositoryDirectory<TId, TModel>(string name)
		where TId : notnull
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Path.Combine(RootPath, "repositories", Encode(GetRepositoryKey<TId, TModel>(name)));
	}

	private string GetJournalDirectory<TEntry>(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Path.Combine(RootPath, "journals", Encode(GetJournalKey<TEntry>(name)));
	}

	private static void CopyDirectory(string source, string target)
	{
		Directory.CreateDirectory(target);
		if (!Directory.Exists(source))
			return;

		foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(source, directory);
			Directory.CreateDirectory(Path.Combine(target, relativePath));
		}

		foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(source, file);
			var targetFile = Path.Combine(target, relativePath);
			Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
			File.Copy(file, targetFile, true);
		}
	}

	private static string GetRepositoryKey<TId, TModel>(string name) =>
		$"{name}|{typeof(TId).AssemblyQualifiedName}|{typeof(TModel).AssemblyQualifiedName}";

	private static string GetJournalKey<TEntry>(string name) => $"{name}|{typeof(TEntry).AssemblyQualifiedName}";

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
