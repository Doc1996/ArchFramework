using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class FieldStateTests
{
	[Fact]
	public void FieldState_DefaultField_IsValid()
	{
		var state = new FieldState<string>("Plan");

		Assert.Equal("Plan", state.Value);
		Assert.False(state.IsTouched);
		Assert.False(state.IsModified);
		Assert.True(state.IsValid);
	}

	[Fact]
	public void FieldState_WithValue_UpdatesValueAndMarksModified()
	{
		var state = new FieldState<string>("Old");
		var updated = state.WithValue("New");

		Assert.Equal("New", updated.Value);
		Assert.True(updated.IsModified);
	}

	[Fact]
	public void FieldState_Touched_MarksTouched()
	{
		var state = new FieldState<string>("Plan");
		var touched = state.Touched();
		Assert.True(touched.IsTouched);
	}

	[Fact]
	public void FieldState_Failed_CarriesTargetedErrorMessage()
	{
		var state = new FieldState<string>("");
		var failed = state.Failed("Name is required.", "plan.name_required", "Name");

		Assert.False(failed.IsValid);
		Assert.Equal(MessageLevel.Error, failed.Messages?.Single().Level);
		Assert.Equal("Name", failed.Messages?.Single().Target);
	}
}
