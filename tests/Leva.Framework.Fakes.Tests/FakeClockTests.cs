using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeClockTests
{
	[Fact]
	public void Advance_ChangesUtcNow()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var clock = new FakeClock(start);

		clock.Advance(TimeSpan.FromMinutes(5));
		Assert.Equal(start.AddMinutes(5), clock.UtcNow);
	}

	[Fact]
	public void Set_ReplacesUtcNow()
	{
		var clock = new FakeClock();
		var value = new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero);

		clock.Set(value);
		Assert.Equal(value, clock.UtcNow);
	}
}
