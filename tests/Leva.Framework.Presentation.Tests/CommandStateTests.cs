using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class CommandStateTests
{
	[Fact]
	public void CommandState_Ready_CanRun()
	{
		var state = CommandState.Ready("Save");

		Assert.Equal(CommandStatus.Ready, state.Status);
		Assert.True(state.CanRun);
		Assert.Equal("Save", state.Label);
	}

	[Fact]
	public void CommandState_Running_CannotRun()
	{
		var state = CommandState.Running("Save", "Saving.");

		Assert.Equal(CommandStatus.Running, state.Status);
		Assert.False(state.CanRun);
		Assert.True(state.IsRunning);
		Assert.Equal(MessageLevel.Info, state.Message?.Level);
	}

	[Fact]
	public void CommandState_Succeeded_CarriesSuccessMessage()
	{
		var state = CommandState.Succeeded("Save", "Saved.");

		Assert.Equal(CommandStatus.Succeeded, state.Status);
		Assert.Equal(MessageLevel.Success, state.Message?.Level);
		Assert.Equal("Saved.", state.Message?.Text);
	}

	[Fact]
	public void CommandState_Failed_CarriesErrorMessage()
	{
		var state = CommandState.Failed("Could not save.", "Save", "plan.save_failed");

		Assert.Equal(CommandStatus.Failed, state.Status);
		Assert.True(state.HasFailed);
		Assert.Equal(MessageLevel.Error, state.Message?.Level);
		Assert.Equal("plan.save_failed", state.Message?.Code);
	}

	[Fact]
	public void CommandState_Disabled_CannotRun()
	{
		var state = CommandState.Disabled("Save", "Nothing changed.");

		Assert.Equal(CommandStatus.Disabled, state.Status);
		Assert.False(state.CanRun);
		Assert.Equal("Nothing changed.", state.Message?.Text);
	}
}
