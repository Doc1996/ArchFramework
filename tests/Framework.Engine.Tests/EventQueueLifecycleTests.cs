using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class EventQueueLifecycleTests
{
	[Fact]
	public async Task DequeueAsync_WaitsUntilEventIsAvailable()
	{
		var queue = CreateQueue();
		var appEvent = new FakeEvent("Later");
		using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));

		var dequeueTask = queue.DequeueAsync(cancellation.Token);
		await Task.Delay(100, cancellation.Token);
		queue.Enqueue(appEvent);

		var queuedEvent = await dequeueTask;
		Assert.Equal(appEvent, queuedEvent.AppEvent);
	}

	[Fact]
	public async Task DequeueAsync_ThrowsWhenCancelledWhileWaiting()
	{
		var queue = CreateQueue();
		using var cancellation = new CancellationTokenSource();
		var dequeueTask = queue.DequeueAsync(cancellation.Token);

		cancellation.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dequeueTask);
	}

	[Fact]
	public async Task EnqueueDelayedAsync_RejectsNegativeDelay()
	{
		var queue = CreateQueue();
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			queue.EnqueueDelayedAsync(new FakeEvent("Bad"), TimeSpan.FromMilliseconds(-1)).AsTask()
		);
	}

	[Fact]
	public async Task EnqueueDelayedAsync_RejectsDuplicateDelayedEventId()
	{
		var queue = CreateQueue();
		var appEvent = new FakeEvent("Duplicate");
		await queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromSeconds(1));

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromSeconds(1)).AsTask()
		);
		Assert.True(queue.Cancel(appEvent.Id));
	}

	[Fact]
	public async Task Cancel_ReturnsFalseAfterDelayedEventWasCancelled()
	{
		var queue = CreateQueue();
		var appEvent = new FakeEvent("Cancel");
		await queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromSeconds(1));

		Assert.True(queue.Cancel(appEvent.Id));
		Assert.False(queue.Cancel(appEvent.Id));
	}

	[Fact]
	public async Task LinkedCancellation_PreventsDelayedEventFromBeingQueued()
	{
		var queue = CreateQueue();
		var appEvent = new FakeEvent("ExternalCancel");
		using var cancellation = new CancellationTokenSource();

		await queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromSeconds(1), token: cancellation.Token);
		cancellation.Cancel();
		await Task.Delay(100);
		Assert.False(queue.TryDequeue(out _));
	}

	[Fact]
	public async Task DelayedEvent_UsesRequestedPriority()
	{
		var queue = CreateQueue();
		var low = new FakeEvent("Low");
		var high = new FakeEvent("High");

		queue.Enqueue(low, EventPriority.Low);
		await queue.EnqueueDelayedAsync(high, TimeSpan.FromMilliseconds(10), EventPriority.High);
		await Task.Delay(100);

		Assert.Equal(high, (await queue.DequeueAsync(CancellationToken.None)).AppEvent);
		Assert.Equal(low, (await queue.DequeueAsync(CancellationToken.None)).AppEvent);
	}

	private static EventQueue CreateQueue()
	{
		var clock = new FakeClock();
		return new EventQueue(clock, new RuntimeLog(clock, new FakeTraceSink()));
	}
}
