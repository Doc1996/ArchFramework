namespace Leva.Framework.Storage.Files.Tests;

internal sealed class TestStorageDirectory : IDisposable
{
	internal TestStorageDirectory()
	{
		Path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			"leva-storage-files-tests-" + Guid.NewGuid().ToString("N")
		);
		Directory.CreateDirectory(Path);
	}

	internal string Path { get; }

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
