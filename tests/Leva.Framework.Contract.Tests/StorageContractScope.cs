using Leva.Framework.Storage;

namespace Leva.Framework.Contract.Tests;

internal sealed class StorageContractScope(
	Func<string, IRepository<string, StorageContractDocument>> createRepository,
	Func<string, IJournal<StorageContractActivity>> createJournal,
	Action? dispose = null
) : IDisposable
{
	public IRepository<string, StorageContractDocument> CreateRepository(string name) => createRepository(name);

	public IJournal<StorageContractActivity> CreateJournal(string name) => createJournal(name);

	public void Dispose() => dispose?.Invoke();
}
