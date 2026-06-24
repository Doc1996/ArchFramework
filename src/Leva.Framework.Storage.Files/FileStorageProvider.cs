using System.Text.Json;
using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Creates file repositories, journals, and storage sessions under one root directory.
/// </summary>
public sealed class FileStorageProvider(string rootPath, JsonSerializerOptions? jsonOptions = null)
	: IStorageSessionFactory
{
	private readonly FileStorageDatabase _database = new FileStorageDatabase(rootPath, jsonOptions);

	public FileRepository<TId, TModel> CreateRepository<TId, TModel>(string name)
		where TId : notnull => new(name, _database);

	public FileRepository<TId, TModel> CreateRepository<TId, TModel>(string name, IStorageSession session)
		where TId : notnull
	{
		var fileSession = RequireSession(session);
		return new FileRepository<TId, TModel>(name, fileSession.Database);
	}

	public FileJournal<TEntry> CreateJournal<TEntry>(string name) => new(name, _database);

	public FileJournal<TEntry> CreateJournal<TEntry>(string name, IStorageSession session)
	{
		var fileSession = RequireSession(session);
		return new FileJournal<TEntry>(name, fileSession.Database);
	}

	public Task<Result<IStorageSession>> OpenAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		try
		{
			Directory.CreateDirectory(_database.RootPath);
			return Task.FromResult(Result<IStorageSession>.Ok(new FileStorageSession(_database)));
		}
		catch (Exception ex)
		{
			return Task.FromResult(Result<IStorageSession>.Fail(StorageErrors.Unavailable("Files", ex.Message)));
		}
	}

	private FileStorageSession RequireSession(IStorageSession session)
	{
		if (session is not FileStorageSession fileSession)
			throw new ArgumentException("The session was not opened by the file provider.", nameof(session));

		if (!fileSession.BelongsTo(_database))
			throw new ArgumentException(
				"The session was opened by a different file storage provider.",
				nameof(session)
			);

		if (fileSession.IsCompleted)
			throw new InvalidOperationException("The storage session is already completed.");

		return fileSession;
	}
}
