using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeRuntimeTests
{
	[Fact]
	public async Task FakeState_RecordsLifecycleCalls()
	{
		var state = new FakeState(new StateId("Ready"), handle: (_, _, _) => Task.FromResult(true));
		var access = new FakeAccess();

		await state.EnterAsync(access, CancellationToken.None);
		var handled = await state.HandleAsync(access, new FakeEvent("Ping"), CancellationToken.None);
		await state.ExitAsync(access, CancellationToken.None);

		Assert.True(handled);
		Assert.Equal(1, state.EnterCount);
		Assert.Equal(1, state.HandleCount);
		Assert.Equal(1, state.ExitCount);
	}

	[Fact]
	public async Task FakeRoutine_UpdatesStatus()
	{
		var routine = new FakeRoutine(new RoutineId("Routine"), handle: (_, _, _) => Task.FromResult(true));
		var access = new FakeAccess();

		await routine.StartAsync(access, CancellationToken.None);
		var handled = await routine.HandleAsync(access, new FakeEvent("Run"), CancellationToken.None);
		routine.Complete();

		Assert.True(handled);
		Assert.Equal(RoutineStatus.Completed, routine.Status);
		Assert.Equal(1, routine.StartCount);
		Assert.Equal(1, routine.HandleCount);
	}

	[Fact]
	public async Task FakeBehavior_RecordsHandleCalls()
	{
		var behavior = new FakeBehavior("Behavior", (_, _, _) => Task.FromResult(true));
		var handled = await behavior.HandleAsync(new FakeAccess(), new FakeEvent("Ping"), CancellationToken.None);

		Assert.True(handled);
		Assert.Equal(1, behavior.HandleCount);
	}

	[Fact]
	public void FakeContext_ProvidesSharedAccessCapabilities()
	{
		var context = new FakeContext();
		context.Access.Transition.To(new StateId("Ready"));
		context.Access.TraceSink.Write(new TraceEntry("Test", "Message", TraceLevel.Info, context.Clock.UtcNow));

		Assert.Single(context.Transition.TransitionEntries);
		Assert.Single(context.TraceSink.TraceEntries);
	}
}
