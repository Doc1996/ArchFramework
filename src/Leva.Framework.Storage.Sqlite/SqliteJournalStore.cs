using System.Data.Common;
using Leva.Framework.Core;
using Microsoft.Data.Sqlite;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Holds SQL operations for one named append-only journal table partition.
/// </summary>
internal sealed class SqliteJournalStore<TEntry>
{
	private readonly string _name;
	private readonly string _journalName;
	private readonly SqliteStorageDatabase _database;

	public SqliteJournalStore(string name, string journalName, SqliteStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(journalName);
		ArgumentNullException.ThrowIfNull(database);

		_name = name;
		_journalName = journalName;
		_database = database;
	}

	public async Task<Result<StorageEntry<TEntry>>> AppendAsync(TEntry value, CancellationToken token)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			return await _database.UseConnectionAsync(
				async connection =>
					await UseTransactionAsync(
						connection,
						async transaction =>
						{
							var version = await GetNextVersionAsync(connection, transaction, token);
							var utcNow = DateTimeOffset.UtcNow;
							var entry = new StorageEntry<TEntry>(value, version, utcNow, utcNow);

							await InsertAsync(connection, transaction, entry, token);

							return Result<StorageEntry<TEntry>>.Ok(entry);
						},
						token
					),
				token
			);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<StorageEntry<TEntry>>.Fail(
				StorageErrors.Failed($"append SQLite journal '{_name}'", ex.Message)
			);
		}
	}

	public async Task<Result<IReadOnlyList<StorageEntry<TEntry>>>> ReadAsync(
		StorageVersion? afterVersion,
		int? limit,
		CancellationToken token
	)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			return await _database.UseConnectionAsync(
				async connection =>
				{
					var entries = new List<StorageEntry<TEntry>>();

					await using var command = connection.CreateCommand();
					command.CommandText = CreateReadCommandText(afterVersion, limit);
					command.Parameters.AddWithValue("$journal_name", _journalName);

					if (afterVersion.HasValue)
						command.Parameters.AddWithValue("$after_version", afterVersion.Value.Value);
					if (limit.HasValue)
						command.Parameters.AddWithValue("$limit", limit.Value);

					using var reader = await command.ExecuteReaderAsync(token);
					while (await reader.ReadAsync(token))
						entries.Add(ReadEntry(reader));

					return Result<IReadOnlyList<StorageEntry<TEntry>>>.Ok(entries);
				},
				token
			);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<IReadOnlyList<StorageEntry<TEntry>>>.Fail(
				StorageErrors.Failed($"read SQLite journal '{_name}'", ex.Message)
			);
		}
	}

	private static async Task<TResult> UseTransactionAsync<TResult>(
		SqliteConnection connection,
		Func<SqliteTransaction, Task<TResult>> action,
		CancellationToken token
	)
	{
		await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token);
		var result = await action(transaction);

		await transaction.CommitAsync(token);
		return result;
	}

	private async Task<StorageVersion> GetNextVersionAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		CancellationToken token
	)
	{
		await using var command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = """
			SELECT COALESCE(MAX(version), 0) + 1
			FROM journal_entries
			WHERE journal_name = $journal_name;
			""";

		command.Parameters.AddWithValue("$journal_name", _journalName);
		var value = (long)(await command.ExecuteScalarAsync(token) ?? 1L);
		return new StorageVersion(value);
	}

	private async Task InsertAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		StorageEntry<TEntry> entry,
		CancellationToken token
	)
	{
		await using var command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = """
			INSERT INTO journal_entries (
				journal_name,
				version,
				value_json,
				created_at,
				updated_at
			)
			VALUES (
				$journal_name,
				$version,
				$value_json,
				$created_at,
				$updated_at
			);
			""";

		command.Parameters.AddWithValue("$journal_name", _journalName);
		command.Parameters.AddWithValue("$version", entry.Version.Value);
		command.Parameters.AddWithValue("$value_json", _database.Serialize(entry.Value));
		command.Parameters.AddWithValue("$created_at", SqliteStorageDatabase.WriteTimestamp(entry.CreatedAt));
		command.Parameters.AddWithValue("$updated_at", SqliteStorageDatabase.WriteTimestamp(entry.UpdatedAt));

		await command.ExecuteNonQueryAsync(token);
	}

	private static string CreateReadCommandText(StorageVersion? afterVersion, int? limit)
	{
		var commandText = """
			SELECT value_json, version, created_at, updated_at
			FROM journal_entries
			WHERE journal_name = $journal_name
			""";

		if (afterVersion.HasValue)
			commandText += " AND version > $after_version";

		commandText += " ORDER BY version";
		if (limit.HasValue)
			commandText += " LIMIT $limit";

		return commandText + ";";
	}

	private StorageEntry<TEntry> ReadEntry(DbDataReader reader) =>
		new(
			_database.Deserialize<TEntry>(reader.GetString(0)),
			new StorageVersion(reader.GetInt64(1)),
			SqliteStorageDatabase.ReadTimestamp(reader.GetString(2)),
			SqliteStorageDatabase.ReadTimestamp(reader.GetString(3))
		);
}
