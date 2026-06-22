using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Dequeues events from the event queue and dispatches them until cancellation is requested.
/// </summary>
public sealed class EventLoop(IEventQueue eventQueue, EventDispatcher eventDispatcher, RuntimeLog runtimeLog)
{
	public async Task RunAsync(CancellationToken token)
	{
		runtimeLog.Add(LogCategory.Event, "Event loop started.");
		try
		{
			while (!token.IsCancellationRequested)
			{
				var queuedEvent = await eventQueue.DequeueAsync(token);
				await eventDispatcher.DispatchAsync(queuedEvent.AppEvent, token);
			}
		}
		finally
		{
			runtimeLog.Add(LogCategory.Event, "Event loop stopped.");
		}
	}

	public async Task RunOneAsync(CancellationToken token = default)
	{
		var queuedEvent = await eventQueue.DequeueAsync(token);
		await eventDispatcher.DispatchAsync(queuedEvent.AppEvent, token);
	}
}
