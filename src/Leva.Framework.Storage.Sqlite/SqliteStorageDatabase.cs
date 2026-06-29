using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Owns database paths, connections, serialization, and key conversion for one SQLite storage provider instance.
/// </summary>
internal sealed class SqliteStorageDatabase
{
	private readonly JsonSerializerOptions _jsonOptions;

	public SqliteStorageDatabase(string databasePath, JsonSerializerOptions? jsonOptions = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
		DatabasePath = Path.GetFullPath(databasePath);
		_jsonOptions = jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
	}

	internal string DatabasePath { get; }

	public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken token)
	{
		var directory = Path.GetDirectoryName(DatabasePath);
		if (!string.IsNullOrWhiteSpace(directory))
			Directory.CreateDirectory(directory);

		var connectionString = new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();
		var connection = new SqliteConnection(connectionString);

		await connection.OpenAsync(token);
		await SqliteStorageInitializer.InitializeAsync(connection, token);
		return connection;
	}

	public SqliteRepositoryStore<TId, TValue> GetRepository<TId, TValue>(string name)
		where TId : notnull => new(name, GetRepositoryName<TId, TValue>(name), this);

	public SqliteJournalStore<TEntry> GetJournal<TEntry>(string name) => new(name, GetJournalName<TEntry>(name), this);

	public async Task<TResult> UseConnectionAsync<TResult>(
		Func<SqliteConnection, Task<TResult>> action,
		CancellationToken token
	)
	{
		await using var connection = await OpenConnectionAsync(token);
		return await action(connection);
	}

	public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _jsonOptions);

	public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _jsonOptions)!;

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

	public static DateTimeOffset ReadTimestamp(string value) =>
		DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

	public static string WriteTimestamp(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

	private static string GetRepositoryName<TId, TValue>(string name)
		where TId : notnull
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Hash($"{name}|{typeof(TId).AssemblyQualifiedName}|{typeof(TValue).AssemblyQualifiedName}");
	}

	private static string GetJournalName<TEntry>(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return Hash($"{name}|{typeof(TEntry).AssemblyQualifiedName}");
	}

	private static string Hash(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
	}
}
