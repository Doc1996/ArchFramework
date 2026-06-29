using Microsoft.Data.Sqlite;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Creates the provider-owned SQLite schema when a database connection is opened.
/// </summary>
internal static class SqliteStorageInitializer
{
	internal static async Task InitializeAsync(SqliteConnection connection, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(connection);

		await ExecuteAsync(
			connection,
			"""
			CREATE TABLE IF NOT EXISTS repository_entries (
				repository_name TEXT NOT NULL,
				storage_id TEXT NOT NULL,
				value_json TEXT NOT NULL,
				version INTEGER NOT NULL,
				created_at TEXT NOT NULL,
				updated_at TEXT NOT NULL,
				PRIMARY KEY (repository_name, storage_id)
			);
			""",
			token
		);

		await ExecuteAsync(
			connection,
			"""
			CREATE TABLE IF NOT EXISTS journal_entries (
				journal_name TEXT NOT NULL,
				version INTEGER NOT NULL,
				value_json TEXT NOT NULL,
				created_at TEXT NOT NULL,
				updated_at TEXT NOT NULL,
				PRIMARY KEY (journal_name, version)
			);
			""",
			token
		);
	}

	private static async Task ExecuteAsync(SqliteConnection connection, string commandText, CancellationToken token)
	{
		await using var command = connection.CreateCommand();
		command.CommandText = commandText;
		await command.ExecuteNonQueryAsync(token);
	}
}
