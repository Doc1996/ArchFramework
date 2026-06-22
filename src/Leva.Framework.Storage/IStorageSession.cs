using Leva.Framework.Core;

namespace Leva.Framework.Storage;

/// <summary>
/// Represents a provider-owned storage boundary for related repository or journal operations.
/// </summary>
public interface IStorageSession : IAsyncDisposable
{
	Task<Result> CommitAsync(CancellationToken token = default);
	Task<Result> RollbackAsync(CancellationToken token = default);
}
