namespace Leva.Framework.Storage.Sqlite.Tests;

internal sealed class TestSqliteDatabase : IDisposable
{
	private readonly string _directoryPath;

	internal TestSqliteDatabase()
	{
		_directoryPath = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			"leva-storage-sqlite-tests-" + Guid.NewGuid().ToString("N")
		);

		Directory.CreateDirectory(_directoryPath);
		Path = System.IO.Path.Combine(_directoryPath, "storage.db");
	}

	internal string Path { get; }

	public void Dispose()
	{
		try
		{
			if (Directory.Exists(_directoryPath))
				Directory.Delete(_directoryPath, true);
		}
		catch (Exception)
		{
			// Ignore cleanup failure to preserve the original failure.
		}
	}
}
