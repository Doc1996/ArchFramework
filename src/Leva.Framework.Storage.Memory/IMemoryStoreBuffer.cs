namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Defines a clonable in-memory store buffer used to isolate storage session changes.
/// </summary>
internal interface IMemoryStoreBuffer
{
	IMemoryStoreBuffer Clone();
}
