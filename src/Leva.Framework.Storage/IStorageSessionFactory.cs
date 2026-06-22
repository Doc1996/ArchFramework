using Leva.Framework.Core;

namespace Leva.Framework.Storage;

/// <summary>
/// Opens provider-owned storage sessions without exposing database-specific transaction details.
/// </summary>
public interface IStorageSessionFactory
{
	Task<Result<IStorageSession>> OpenAsync(CancellationToken token = default);
}
