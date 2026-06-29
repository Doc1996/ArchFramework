using System.Data.Common;
using Leva.Framework.Core;
using Microsoft.Data.Sqlite;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Holds SQL operations for one named repository table partition.
/// </summary>
internal sealed class SqliteRepositoryStore<TId, TValue>
	where TId : notnull
{
	private readonly string _name;
	private readonly string _repositoryName;
	private readonly SqliteStorageDatabase _database;

	internal SqliteRepositoryStore(string name, string repositoryName, SqliteStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(repositoryName);
		ArgumentNullException.ThrowIfNull(database);

		_name = name;
		_repositoryName = repositoryName;
		_database = database;
	}

	internal async Task<Result<StorageEntry<TValue>>> SaveAsync(
		TId id,
		TValue value,
		StorageVersion? expectedVersion,
		CancellationToken token
	)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			var key = SqliteStorageDatabase.ToKey(id);

			return await _database.UseConnectionAsync(
				async connection =>
					await UseTransactionAsync(
						connection,
						async transaction =>
							await SaveAsync(connection, transaction, key, value, expectedVersion, token),
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
			return Result<StorageEntry<TValue>>.Fail(
				StorageErrors.Failed($"save SQLite repository '{_name}'", ex.Message)
			);
		}
	}

	internal async Task<Result<StorageEntry<TValue>>> LoadAsync(TId id, CancellationToken token)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			var key = SqliteStorageDatabase.ToKey(id);

			return await _database.UseConnectionAsync(
				async connection =>
				{
					var existing = await LoadExistingAsync(connection, null, key, token);
					return existing.HasValue
						? Result<StorageEntry<TValue>>.Ok(existing.Value)
						: Result<StorageEntry<TValue>>.Fail(StorageErrors.NotFound(_name, key));
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
			return Result<StorageEntry<TValue>>.Fail(
				StorageErrors.Failed($"load SQLite repository '{_name}'", ex.Message)
			);
		}
	}

	internal async Task<Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>> LoadAllAsync(CancellationToken token)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			return await _database.UseConnectionAsync(
				async connection =>
				{
					var entries = new Dictionary<TId, StorageEntry<TValue>>();
					await using var command = connection.CreateCommand();

					command.CommandText = """
					SELECT storage_id, value_json, version, created_at, updated_at
					FROM repository_entries
					WHERE repository_name = $repository_name
					ORDER BY storage_id;
					""";

					command.Parameters.AddWithValue("$repository_name", _repositoryName);
					using var reader = await command.ExecuteReaderAsync(token);

					while (await reader.ReadAsync(token))
					{
						var key = reader.GetString(0);
						entries[SqliteStorageDatabase.FromKey<TId>(key)] = ReadEntry(reader, 1);
					}

					return Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>.Ok(entries);
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
			return Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>.Fail(
				StorageErrors.Failed($"load SQLite repository '{_name}'", ex.Message)
			);
		}
	}

	internal async Task<Result<bool>> ExistsAsync(TId id, CancellationToken token)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			var key = SqliteStorageDatabase.ToKey(id);

			return await _database.UseConnectionAsync(
				async connection =>
				{
					await using var command = connection.CreateCommand();
					command.CommandText = """
					SELECT 1
					FROM repository_entries
					WHERE repository_name = $repository_name AND storage_id = $storage_id
					LIMIT 1;
					""";

					command.Parameters.AddWithValue("$repository_name", _repositoryName);
					command.Parameters.AddWithValue("$storage_id", key);

					var result = await command.ExecuteScalarAsync(token);
					return Result<bool>.Ok(result is not null);
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
			return Result<bool>.Fail(StorageErrors.Failed($"check SQLite repository '{_name}'", ex.Message));
		}
	}

	internal async Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion, CancellationToken token)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			var key = SqliteStorageDatabase.ToKey(id);

			return await _database.UseConnectionAsync(
				async connection =>
					await UseTransactionAsync(
						connection,
						async transaction => await DeleteAsync(connection, transaction, key, expectedVersion, token),
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
			return Result.Fail(StorageErrors.Failed($"delete SQLite repository '{_name}'", ex.Message));
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

	private async Task<Result<StorageEntry<TValue>>> SaveAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		string key,
		TValue value,
		StorageVersion? expectedVersion,
		CancellationToken token
	)
	{
		var existing = await LoadExistingAsync(connection, transaction, key, token);
		var utcNow = DateTimeOffset.UtcNow;

		if (existing.HasValue)
			return await UpdateExistingAsync(
				connection,
				transaction,
				key,
				value,
				existing.Value,
				expectedVersion,
				utcNow,
				token
			);

		if (expectedVersion.HasValue)
			return Result<StorageEntry<TValue>>.Fail(StorageErrors.NotFound(_name, key));

		var newEntry = new StorageEntry<TValue>(value, new StorageVersion(1), utcNow, utcNow);
		await InsertAsync(connection, transaction, key, newEntry, token);
		return Result<StorageEntry<TValue>>.Ok(newEntry);
	}

	private async Task<Result<StorageEntry<TValue>>> UpdateExistingAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		string key,
		TValue value,
		StorageEntry<TValue> existing,
		StorageVersion? expectedVersion,
		DateTimeOffset utcNow,
		CancellationToken token
	)
	{
		if (expectedVersion.HasValue && existing.Version != expectedVersion.Value)
			return Result<StorageEntry<TValue>>.Fail(
				StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Version)
			);

		var updatedEntry = existing with { Value = value, Version = existing.Version.Next(), UpdatedAt = utcNow };
		await UpdateAsync(connection, transaction, key, updatedEntry, token);
		return Result<StorageEntry<TValue>>.Ok(updatedEntry);
	}

	private async Task<Result> DeleteAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		string key,
		StorageVersion? expectedVersion,
		CancellationToken token
	)
	{
		var existing = await LoadExistingAsync(connection, transaction, key, token);
		if (!existing.HasValue)
			return Result.Fail(StorageErrors.NotFound(_name, key));

		if (expectedVersion.HasValue && existing.Value.Version != expectedVersion.Value)
			return Result.Fail(
				StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Value.Version)
			);

		await DeleteExistingAsync(connection, transaction, key, token);
		return Result.Ok();
	}

	private async Task<StorageEntry<TValue>?> LoadExistingAsync(
		SqliteConnection connection,
		SqliteTransaction? transaction,
		string key,
		CancellationToken token
	)
	{
		await using var command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = """
			SELECT value_json, version, created_at, updated_at
			FROM repository_entries
			WHERE repository_name = $repository_name AND storage_id = $storage_id;
			""";

		command.Parameters.AddWithValue("$repository_name", _repositoryName);
		command.Parameters.AddWithValue("$storage_id", key);

		using var reader = await command.ExecuteReaderAsync(token);
		return await reader.ReadAsync(token) ? ReadEntry(reader, 0) : null;
	}

	private async Task InsertAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		string key,
		StorageEntry<TValue> entry,
		CancellationToken token
	)
	{
		await using var command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = """
			INSERT INTO repository_entries (
				repository_name,
				storage_id,
				value_json,
				version,
				created_at,
				updated_at
			)
			VALUES (
				$repository_name,
				$storage_id,
				$value_json,
				$version,
				$created_at,
				$updated_at
			);
			""";

		AddEntryParameters(command, key, entry);
		await command.ExecuteNonQueryAsync(token);
	}

	private async Task UpdateAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		string key,
		StorageEntry<TValue> entry,
		CancellationToken token
	)
	{
		await using var command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = """
			UPDATE repository_entries
			SET value_json = $value_json,
				version = $version,
				updated_at = $updated_at
			WHERE repository_name = $repository_name AND storage_id = $storage_id;
			""";

		command.Parameters.AddWithValue("$repository_name", _repositoryName);
		command.Parameters.AddWithValue("$storage_id", key);
		command.Parameters.AddWithValue("$value_json", _database.Serialize(entry.Value));
		command.Parameters.AddWithValue("$version", entry.Version.Value);
		command.Parameters.AddWithValue("$updated_at", SqliteStorageDatabase.WriteTimestamp(entry.UpdatedAt));

		await command.ExecuteNonQueryAsync(token);
	}

	private async Task DeleteExistingAsync(
		SqliteConnection connection,
		SqliteTransaction transaction,
		string key,
		CancellationToken token
	)
	{
		await using var command = connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = """
			DELETE FROM repository_entries
			WHERE repository_name = $repository_name AND storage_id = $storage_id;
			""";

		command.Parameters.AddWithValue("$repository_name", _repositoryName);
		command.Parameters.AddWithValue("$storage_id", key);
		await command.ExecuteNonQueryAsync(token);
	}

	private void AddEntryParameters(SqliteCommand command, string key, StorageEntry<TValue> entry)
	{
		command.Parameters.AddWithValue("$repository_name", _repositoryName);
		command.Parameters.AddWithValue("$storage_id", key);
		command.Parameters.AddWithValue("$value_json", _database.Serialize(entry.Value));
		command.Parameters.AddWithValue("$version", entry.Version.Value);
		command.Parameters.AddWithValue("$created_at", SqliteStorageDatabase.WriteTimestamp(entry.CreatedAt));
		command.Parameters.AddWithValue("$updated_at", SqliteStorageDatabase.WriteTimestamp(entry.UpdatedAt));
	}

	private StorageEntry<TValue> ReadEntry(DbDataReader reader, int valueIndex) =>
		new(
			_database.Deserialize<TValue>(reader.GetString(valueIndex)),
			new StorageVersion(reader.GetInt64(valueIndex + 1)),
			SqliteStorageDatabase.ReadTimestamp(reader.GetString(valueIndex + 2)),
			SqliteStorageDatabase.ReadTimestamp(reader.GetString(valueIndex + 3))
		);
}
