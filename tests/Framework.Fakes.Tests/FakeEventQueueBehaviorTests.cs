using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeEventQueueBehaviorTests
{
	[Fact]
	public async Task DequeueAsync_WaitsUntilEventIsEnqueued()
	{
		var queue = new FakeEventQueue();
		var appEvent = new FakeEvent("Later");
		using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));

		var dequeueTask = queue.DequeueAsync(cancellation.Token);
		await Task.Delay(20, cancellation.Token);
		queue.Enqueue(appEvent);

		var queuedEvent = await dequeueTask;
		Assert.Equal(appEvent, queuedEvent.AppEvent);
	}

	[Fact]
	public async Task DequeueAsync_ThrowsWhenCancelled()
	{
		var queue = new FakeEventQueue();
		using var cancellation = new CancellationTokenSource();
		var dequeueTask = queue.DequeueAsync(cancellation.Token);

		cancellation.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dequeueTask);
	}

	[Fact]
	public async Task DequeueAsync_UsesPriorityOrder()
	{
		var queue = new FakeEventQueue();
		var low = new FakeEvent("Low");
		var critical = new FakeEvent("Critical");

		queue.Enqueue(low, EventPriority.Low);
		queue.Enqueue(critical, EventPriority.Critical);

		Assert.Equal(critical, (await queue.DequeueAsync(CancellationToken.None)).AppEvent);
		Assert.Equal(low, (await queue.DequeueAsync(CancellationToken.None)).AppEvent);
	}

	[Fact]
	public async Task EnqueueDelayedAsync_EnqueuesImmediatelyForDeterministicTests()
	{
		var queue = new FakeEventQueue();
		var appEvent = new FakeEvent("Delayed");

		await queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromHours(1));
		Assert.True(queue.TryDequeue(out var queuedEvent));
		Assert.Equal(appEvent, queuedEvent?.AppEvent);
	}

	[Fact]
	public void Clear_RemovesQueuedEvents()
	{
		var queue = new FakeEventQueue();
		queue.Enqueue(new FakeEvent("One"));
		queue.Enqueue(new FakeEvent("Two"));
		queue.Clear();

		Assert.Equal(0, queue.Count);
		Assert.False(queue.TryDequeue(out _));
	}
}
