using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class ViewStateTests
{
	[Fact]
	public void ViewState_Idle_CreatesIdleState()
	{
		var state = ViewState<string>.Idle();

		Assert.Equal(ViewStatus.Idle, state.Status);
		Assert.False(state.IsLoading);
		Assert.False(state.IsReady);
		Assert.Null(state.Value);
	}

	[Fact]
	public void ViewState_Loading_CreatesLoadingStateWithMessage()
	{
		var state = ViewState<string>.Loading("Loading plan.");

		Assert.Equal(ViewStatus.Loading, state.Status);
		Assert.True(state.IsLoading);
		Assert.Equal(MessageLevel.Info, state.Message?.Level);
		Assert.Equal("Loading plan.", state.Message?.Text);
	}

	[Fact]
	public void ViewState_Ready_CreatesReadyStateWithValue()
	{
		var state = ViewState<string>.Ready("plan", "Plan loaded.");

		Assert.Equal(ViewStatus.Ready, state.Status);
		Assert.True(state.IsReady);
		Assert.Equal("plan", state.Value);
		Assert.Equal(MessageLevel.Success, state.Message?.Level);
	}

	[Fact]
	public void ViewState_Empty_CreatesEmptyState()
	{
		var state = ViewState<string>.Empty("No plans yet.");

		Assert.Equal(ViewStatus.Empty, state.Status);
		Assert.True(state.IsEmpty);
		Assert.Equal("No plans yet.", state.Message?.Text);
	}

	[Fact]
	public void ViewState_Failed_CreatesFailedState()
	{
		var state = ViewState<string>.Failed("Could not load plan.", "plan.load_failed");

		Assert.Equal(ViewStatus.Failed, state.Status);
		Assert.True(state.HasFailed);
		Assert.Equal(MessageLevel.Error, state.Message?.Level);
		Assert.Equal("plan.load_failed", state.Message?.Code);
	}
}
