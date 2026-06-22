using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Defines the engine boundary for immediate, delayed, cancelled, and dequeued events.
/// </summary>
public interface IEventQueue
{
	int Count { get; }

	EventId Enqueue(IEvent appEvent, EventPriority priority = EventPriority.Normal);
	ValueTask<EventId> EnqueueDelayedAsync(
		IEvent appEvent,
		TimeSpan delay,
		EventPriority priority = EventPriority.Normal,
		CancellationToken token = default
	);

	bool Cancel(EventId eventId);
	Task<QueuedEvent> DequeueAsync(CancellationToken token);
	bool TryDequeue(out QueuedEvent? queuedEvent);
}
