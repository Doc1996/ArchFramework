using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Simple event value for tests.
/// </summary>
public sealed record FakeEvent(EventId Id, string Name) : IEvent
{
	public FakeEvent(string name)
		: this(EventId.New(), name) { }
}
