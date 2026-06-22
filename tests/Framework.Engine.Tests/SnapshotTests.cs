using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class SnapshotTests
{
	[Fact]
	public async Task CreateSnapshot_StoresRuntimeCollectionsInData()
	{
		var state = new FakeState(new StateId("Ready"));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state]);
		var alarm = new AlarmEntry(AlarmId.New(), "Alarm", AlarmLevel.Warning, false, fake.Clock.UtcNow);
		var status = new StatusEntry("Device", "Connected", true, fake.Clock.UtcNow);
		var command = context.CommandBoard.Start(CommandId.New(), "Command").Complete();

		await context.StateMachine.StartAsync(state.Id);
		context.AlarmBoard.Raise(alarm);
		context.StatusBoard.Set(status);
		var snapshot = context.CreateSnapshot(new Dictionary<string, object?> { ["App"] = "Value" });

		Assert.Equal(state.Id, snapshot.StateId);
		Assert.Equal("Value", snapshot.Data["App"]);
		Assert.Contains(alarm, Assert.IsAssignableFrom<IEnumerable<AlarmEntry>>(snapshot.Data["Alarms"]));
		Assert.Contains(status, Assert.IsAssignableFrom<IEnumerable<StatusEntry>>(snapshot.Data["Statuses"]));
		Assert.Contains(command, Assert.IsAssignableFrom<IEnumerable<CommandEntry>>(snapshot.Data["Commands"]));
	}

	[Fact]
	public async Task LoadSnapshot_RestoresStateAndBoards()
	{
		var ready = new FakeState(new StateId("Ready"));
		var restored = new FakeState(new StateId("Restored"));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [ready, restored]);
		var alarm = new AlarmEntry(AlarmId.New(), "Alarm", AlarmLevel.Error, true, fake.Clock.UtcNow);
		var status = new StatusEntry("Device", "Mode", "Auto", fake.Clock.UtcNow);

		var command = new CommandEntry(
			CommandId.New(),
			"Command",
			CommandStatus.Started,
			fake.Clock.UtcNow,
			fake.Clock.UtcNow
		);
		var snapshot = new Snapshot(
			restored.Id,
			fake.Clock.UtcNow,
			new Dictionary<string, object?>
			{
				["Alarms"] = new[] { alarm },
				["Statuses"] = new[] { status },
				["Commands"] = new[] { command },
			}
		);

		await context.StateMachine.StartAsync(ready.Id);
		await context.LoadSnapshotAsync(snapshot);

		Assert.Equal(restored.Id, context.StateMachine.CurrentStateId);
		Assert.Contains(alarm, context.AlarmBoard.AlarmEntries);
		Assert.Contains(status, context.StatusBoard.StatusEntries);
		Assert.Contains(command, context.CommandBoard.CommandEntries);
	}
}
