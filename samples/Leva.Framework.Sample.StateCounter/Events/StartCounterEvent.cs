using Leva.Framework.Core;
using FrameworkEventId = Leva.Framework.Core.EventId;

namespace Leva.Framework.Sample.StateCounter;

internal sealed record StartCounterEvent(FrameworkEventId Id) : IEvent
{
	public string Name => nameof(StartCounterEvent);
}
