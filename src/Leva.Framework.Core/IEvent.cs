namespace Leva.Framework.Core;

/// <summary>
/// Represents a typed application fact that can be queued and handled by the runtime.
/// </summary>
public interface IEvent
{
	EventId Id { get; }
	string Name { get; }
}
