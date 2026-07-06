using Leva.Framework.Core;
using Leva.Framework.Engine;

namespace Leva.Framework.Fakes;

/// <summary>
/// Deterministic in-memory event queue for tests.
/// </summary>
public sealed class FakeEventQueue(IClock? clock = null) : IEventQueue
{
	private readonly Queue<QueuedEvent> _critical = new();
	private readonly Queue<QueuedEvent> _high = new();
	private readonly Queue<QueuedEvent> _normal = new();
	private readonly Queue<QueuedEvent> _low = new();

	private readonly IClock _clock = clock ?? new FakeClock();
	private readonly SemaphoreSlim _asyncLock = new(0);
	public int Count => _critical.Count + _high.Count + _normal.Count + _low.Count;

	public EventId Enqueue(IEvent appEvent, EventPriority priority = EventPriority.Normal)
	{
		var queuedEvent = new QueuedEvent(appEvent, priority, _clock.UtcNow);
		GetQueue(priority).Enqueue(queuedEvent);
		_asyncLock.Release();

		return appEvent.Id;
	}

	public ValueTask<EventId> EnqueueDelayedAsync(
		IEvent appEvent,
		TimeSpan delay,
		EventPriority priority = EventPriority.Normal,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		Enqueue(appEvent, priority);
		return ValueTask.FromResult(appEvent.Id);
	}

	public bool Cancel(EventId eventId) => false;

	public async Task<QueuedEvent> DequeueAsync(CancellationToken token)
	{
		while (true)
		{
			await _asyncLock.WaitAsync(token);
			if (TryDequeue(out var queuedEvent) && queuedEvent is not null)
				return queuedEvent;
		}
	}

	public bool TryDequeue(out QueuedEvent? queuedEvent)
	{
		if (_critical.TryDequeue(out queuedEvent))
			return true;
		if (_high.TryDequeue(out queuedEvent))
			return true;
		if (_normal.TryDequeue(out queuedEvent))
			return true;
		if (_low.TryDequeue(out queuedEvent))
			return true;

		queuedEvent = null;
		return false;
	}

	public void Clear()
	{
		while (TryDequeue(out _)) { }
	}

	private Queue<QueuedEvent> GetQueue(EventPriority priority) =>
		priority switch
		{
			EventPriority.Critical => _critical,
			EventPriority.High => _high,
			EventPriority.Normal => _normal,
			EventPriority.Low => _low,
			_ => _normal,
		};
}
