using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeEventQueueTests
{
	[Fact]
	public async Task EnqueueAndDequeue_ReturnsEvent()
	{
		var queue = new FakeEventQueue();
		var appEvent = new FakeEvent("Ping");

		var eventId = queue.Enqueue(appEvent, EventPriority.High);
		var queuedEvent = await queue.DequeueAsync(CancellationToken.None);

		Assert.Equal(appEvent.Id, eventId);
		Assert.Equal(appEvent, queuedEvent.AppEvent);
		Assert.Equal(EventPriority.High, queuedEvent.Priority);
	}

	[Fact]
	public void TryDequeue_ReturnsFalseWhenEmpty()
	{
		var queue = new FakeEventQueue();
		Assert.False(queue.TryDequeue(out var queuedEvent));
		Assert.Null(queuedEvent);
	}
}
