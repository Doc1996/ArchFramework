using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Runs serialized async file storage operations and converts unexpected exceptions to storage failures.
/// </summary>
internal sealed class FileStorageRunner(string source)
{
	private readonly SemaphoreSlim _asyncLock = new(1, 1);

	public async Task<Result<TValue>> RunAsync<TValue>(
		string operation,
		Func<Task<Result<TValue>>> action,
		CancellationToken token
	)
	{
		await _asyncLock.WaitAsync(token);

		try
		{
			return await action();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<TValue>.Fail(StorageErrors.Failed($"{operation} {source}", ex.Message));
		}
		finally
		{
			_asyncLock.Release();
		}
	}

	public async Task<Result> RunAsync(string operation, Func<Task<Result>> action, CancellationToken token)
	{
		await _asyncLock.WaitAsync(token);

		try
		{
			return await action();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result.Fail(StorageErrors.Failed($"{operation} {source}", ex.Message));
		}
		finally
		{
			_asyncLock.Release();
		}
	}
}
