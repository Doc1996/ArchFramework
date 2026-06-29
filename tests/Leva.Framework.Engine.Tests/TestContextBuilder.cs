using Leva.Framework.Core;
using Leva.Framework.Fakes;

namespace Leva.Framework.Engine.Tests;

internal static class TestContextBuilder
{
	internal static Context Create(
		FakeContext fake,
		IEnumerable<IState<FakeAccess>>? states = null,
		IEnumerable<IBehavior<FakeAccess>>? behaviors = null,
		IAlarmSupervisor? alarmSupervisor = null,
		IStatusUpdater? statusUpdater = null
	)
	{
		var builder = new ContextBuilder()
			.WithClock(fake.Clock)
			.WithLogSink(fake.LogSink)
			.WithEventQueue(fake.EventQueue);

		if (alarmSupervisor is not null)
			builder.WithAlarmSupervisor(alarmSupervisor);

		if (statusUpdater is not null)
			builder.WithStatusUpdater(statusUpdater);

		foreach (var state in states ?? [])
			builder.AddState(state, access => new FakeAccess(access));

		foreach (var behavior in behaviors ?? [])
			builder.AddBehavior(behavior, access => new FakeAccess(access));

		return builder.Build();
	}
}
