using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class SnapshotEdgeTests
{
	[Fact]
	public void CreateSnapshot_ThrowsBeforeStateMachineStarts()
	{
		var state = new FakeState(new StateId("Ready"));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state]);

		Assert.Throws<InvalidOperationException>(() => context.CreateSnapshot());
	}

	[Fact]
	public async Task LoadSnapshot_AcceptsSingleItemsInData()
	{
		var ready = new FakeState(new StateId("Ready"));
		var restored = new FakeState(new StateId("Restored"));
		var fake = new FakeContext();

		var context = TestContextBuilder.Create(fake, [ready, restored]);
		var alarm = new AlarmEntry(AlarmId.New(), "Alarm", AlarmLevel.Error, true, fake.Clock.UtcNow);
		var status = new StatusEntry("Device", "Mode", "Auto", fake.Clock.UtcNow);

		var snapshot = new Snapshot(
			restored.Id,
			fake.Clock.UtcNow,
			new Dictionary<string, object?> { ["Alarms"] = alarm, ["Statuses"] = status }
		);

		await context.StateMachine.StartAsync(ready.Id);
		await context.LoadSnapshotAsync(snapshot);

		Assert.Contains(alarm, context.AlarmBoard.AlarmEntries);
		Assert.Contains(status, context.StatusBoard.StatusEntries);
	}

	[Fact]
	public async Task LoadSnapshot_ReplacesExistingBoardEntries()
	{
		var ready = new FakeState(new StateId("Ready"));
		var restored = new FakeState(new StateId("Restored"));
		var fake = new FakeContext();

		var context = TestContextBuilder.Create(fake, [ready, restored]);
		var oldAlarm = new AlarmEntry(AlarmId.New(), "Old", AlarmLevel.Warning, false, fake.Clock.UtcNow);
		var newAlarm = new AlarmEntry(AlarmId.New(), "New", AlarmLevel.Critical, true, fake.Clock.UtcNow);

		var snapshot = new Snapshot(
			restored.Id,
			fake.Clock.UtcNow,
			new Dictionary<string, object?> { ["Alarms"] = new[] { newAlarm } }
		);

		await context.StateMachine.StartAsync(ready.Id);
		context.AlarmBoard.Raise(oldAlarm);
		await context.LoadSnapshotAsync(snapshot);

		Assert.DoesNotContain(oldAlarm, context.AlarmBoard.AlarmEntries);
		Assert.Contains(newAlarm, context.AlarmBoard.AlarmEntries);
	}
}
