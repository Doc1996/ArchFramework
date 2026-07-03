using Leva.Framework.Storage.Files;
using Leva.Framework.Storage.Memory;
using Leva.Framework.Storage.Sqlite;

namespace Leva.Framework.Contract.Tests;

internal static class StorageContractProviders
{
	public static StorageContractScope Create(string providerName) =>
		providerName switch
		{
			"memory" => CreateMemory(),
			"files" => CreateFiles(),
			"sqlite" => CreateSqlite(),
			_ => throw new ArgumentOutOfRangeException(nameof(providerName), providerName, "Unknown storage provider."),
		};

	private static StorageContractScope CreateMemory()
	{
		var provider = new MemoryStorageProvider();
		return new StorageContractScope(
			name => provider.CreateRepository<string, StorageContractDocument>(name),
			name => provider.CreateJournal<StorageContractActivity>(name)
		);
	}

	private static StorageContractScope CreateFiles()
	{
		var directoryPath = CreateTemporaryDirectory("leva-contract-storage-files-");
		var provider = new FileStorageProvider(directoryPath);

		return new StorageContractScope(
			name => provider.CreateRepository<string, StorageContractDocument>(name),
			name => provider.CreateJournal<StorageContractActivity>(name),
			() => DeleteDirectory(directoryPath)
		);
	}

	private static StorageContractScope CreateSqlite()
	{
		var directoryPath = CreateTemporaryDirectory("leva-contract-storage-sqlite-");
		var databasePath = Path.Combine(directoryPath, "storage.db");
		var provider = new SqliteStorageProvider(databasePath);

		return new StorageContractScope(
			name => provider.CreateRepository<string, StorageContractDocument>(name),
			name => provider.CreateJournal<StorageContractActivity>(name),
			() => DeleteDirectory(directoryPath)
		);
	}

	private static string CreateTemporaryDirectory(string prefix)
	{
		var directoryPath = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(directoryPath);
		return directoryPath;
	}

	private static void DeleteDirectory(string directoryPath)
	{
		try
		{
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}
		catch (Exception)
		{
			// Preserve the original test failure if cleanup fails.
		}
	}
}
