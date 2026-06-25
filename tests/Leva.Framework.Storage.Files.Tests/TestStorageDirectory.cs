namespace Leva.Framework.Storage.Files.Tests;

internal sealed class TestStorageDirectory : IDisposable
{
	public TestStorageDirectory()
	{
		Path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			"leva-storage-files-tests-" + Guid.NewGuid().ToString("N")
		);
		Directory.CreateDirectory(Path);
	}

	public string Path { get; }

	public void Dispose()
	{
		try
		{
			if (Directory.Exists(Path))
				Directory.Delete(Path, true);
		}
		catch (Exception)
		{
			// Ignore cleanup failure to preserve the original failure.
		}
	}
}
