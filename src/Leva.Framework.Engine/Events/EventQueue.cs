using System.Collections.Concurrent;
using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Queues immediate and delayed events, orders them by priority, and supports cancellation of delayed events.
/// </summary>
public sealed class EventQueue(IClock clock, RuntimeLog runtimeLog) : IEventQueue
{
	private readonly ConcurrentQueue<QueuedEvent> _critical = new();
	private readonly ConcurrentQueue<QueuedEvent> _high = new();
	private readonly ConcurrentQueue<QueuedEvent> _normal = new();
	private readonly ConcurrentQueue<QueuedEvent> _low = new();

	private readonly ConcurrentDictionary<EventId, CancellationTokenSource> _delayedSources = new();
	private readonly SemaphoreSlim _asyncLock = new(0);
	public int Count => _critical.Count + _high.Count + _normal.Count + _low.Count;

	public EventId Enqueue(IEvent appEvent, EventPriority priority = EventPriority.Normal)
	{
		var queuedEvent = new QueuedEvent(appEvent, priority, clock.UtcNow);
		GetQueue(priority).Enqueue(queuedEvent);
		_asyncLock.Release();

		LogEvent("Event enqueued.", appEvent, priority);
		return appEvent.Id;
	}

	public ValueTask<EventId> EnqueueDelayedAsync(
		IEvent appEvent,
		TimeSpan delay,
		EventPriority priority = EventPriority.Normal,
		CancellationToken token = default
	)
	{
		if (delay < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(delay), "Delay must not be negative.");

		var delayedSource = CancellationTokenSource.CreateLinkedTokenSource(token);
		if (!_delayedSources.TryAdd(appEvent.Id, delayedSource))
		{
			delayedSource.Dispose();
			throw new InvalidOperationException($"Event '{appEvent.Id.Value}' is already scheduled.");
		}

		LogEvent("Event delayed.", appEvent, priority, new { Delay = delay });
		_ = RunDelayedAsync(appEvent, delay, priority, delayedSource);

		return ValueTask.FromResult(appEvent.Id);
	}

	public bool Cancel(EventId eventId)
	{
		if (!_delayedSources.TryRemove(eventId, out var delayedSource))
			return false;

		delayedSource.Cancel();
		return true;
	}

	public async Task<QueuedEvent> DequeueAsync(CancellationToken token)
	{
		while (true)
		{
			await _asyncLock.WaitAsync(token);
			if (TryDequeue(out var queuedEvent) && queuedEvent is not null)
			{
				LogEvent("Event dequeued.", queuedEvent.AppEvent, queuedEvent.Priority);
				return queuedEvent;
			}
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

	private async Task RunDelayedAsync(
		IEvent appEvent,
		TimeSpan delay,
		EventPriority priority,
		CancellationTokenSource delayedSource
	)
	{
		try
		{
			await Task.Delay(delay, delayedSource.Token);
			if (_delayedSources.TryRemove(appEvent.Id, out _))
				Enqueue(appEvent, priority);
		}
		catch (OperationCanceledException)
		{
			LogEvent("Delayed event cancelled.", appEvent, priority);
		}
		catch (Exception exception)
		{
			LogEvent("Delayed event failed.", appEvent, priority, new { Error = exception.Message });
		}
		finally
		{
			_delayedSources.TryRemove(appEvent.Id, out _);
			delayedSource.Dispose();
		}
	}

	private ConcurrentQueue<QueuedEvent> GetQueue(EventPriority priority) =>
		priority switch
		{
			EventPriority.Critical => _critical,
			EventPriority.High => _high,
			EventPriority.Normal => _normal,
			EventPriority.Low => _low,
			_ => _normal,
		};

	private void LogEvent(string message, IEvent appEvent, EventPriority priority, object? additionalDetails = null) =>
		runtimeLog.Add(LogCategory.Event, message, appEvent, new { Priority = priority }, additionalDetails);
}
