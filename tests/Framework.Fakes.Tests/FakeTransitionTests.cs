using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeTransitionTests
{
	[Fact]
	public void To_CapturesTransitionTargetAndReason()
	{
		var fakeTransition = new FakeTransition();
		var stateId = new StateId("Ready");
		fakeTransition.To(stateId, "done");

		var transitionEntry = Assert.Single(fakeTransition.TransitionEntries);
		Assert.Equal("To", transitionEntry.Source);
		Assert.Equal(stateId, transitionEntry.StateId);
		Assert.Equal("done", transitionEntry.Reason);
	}

	[Fact]
	public void Reenter_CapturesReenterRequest()
	{
		var fakeTransition = new FakeTransition();
		fakeTransition.Reenter("reload");

		var transitionEntry = Assert.Single(fakeTransition.TransitionEntries);
		Assert.Equal("Reenter", transitionEntry.Source);
		Assert.Null(transitionEntry.StateId);
		Assert.Equal("reload", transitionEntry.Reason);
	}

	[Fact]
	public void Clear_RemovesTransitionEntries()
	{
		var fakeTransition = new FakeTransition();
		fakeTransition.To(new StateId("Ready"));
		fakeTransition.Clear();

		Assert.Empty(fakeTransition.TransitionEntries);
	}
}
