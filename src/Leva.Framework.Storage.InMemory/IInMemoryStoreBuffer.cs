namespace Leva.Framework.Storage.InMemory;

/// <summary>
/// Defines clonable in-memory store buffer used to isolate storage session changes.
/// </summary>
internal interface IInMemoryStoreBuffer
{
	IInMemoryStoreBuffer Clone();
}
