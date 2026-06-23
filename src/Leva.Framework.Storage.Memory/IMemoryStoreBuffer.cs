namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Defines an in-memory store buffer that can be copied for storage sessions.
/// </summary>
internal interface IMemoryStoreBuffer
{
	IMemoryStoreBuffer Clone();

	void CopyFrom(IMemoryStoreBuffer source);
}
