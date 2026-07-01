using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Core.Tests;

public sealed class IdTests
{
	[Fact]
	public void RuntimeIds_NewCreatesDifferentValues()
	{
		Assert.NotEqual(AlarmId.New(), AlarmId.New());
		Assert.NotEqual(EventId.New(), EventId.New());
		Assert.NotEqual(RequestId.New(), RequestId.New());
	}

	[Fact]
	public void NamedIds_KeepProvidedValues()
	{
		Assert.Equal("Ready", new StateId("Ready").Value);
		Assert.Equal("Calibration", new RoutineId("Calibration").Value);
	}
}
