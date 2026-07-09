using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class FormStateTests
{
	[Fact]
	public void FormState_Clean_CanSubmit()
	{
		var state = FormState.Clean();

		Assert.Equal(FormStatus.Clean, state.Status);
		Assert.True(state.CanSubmit);
		Assert.True(state.IsValid);
	}

	[Fact]
	public void FormState_Submitting_CannotSubmit()
	{
		var state = FormState.Submitting();

		Assert.Equal(FormStatus.Submitting, state.Status);
		Assert.False(state.CanSubmit);
		Assert.True(state.IsSubmitting);
	}

	[Fact]
	public void FormState_Submitted_CarriesSuccessMessage()
	{
		var state = FormState.Submitted("Submitted.");
		Assert.Equal(FormStatus.Submitted, state.Status);
		Assert.Equal(MessageLevel.Success, state.Messages?.Single().Level);
	}

	[Fact]
	public void FormState_Invalid_CarriesValidationMessages()
	{
		var messages = new[] { MessageEntry.Error("Name is required.", target: "Name") };
		var state = FormState.Invalid(messages);

		Assert.Equal(FormStatus.Invalid, state.Status);
		Assert.False(state.IsValid);
		Assert.Equal("Name", state.Messages?.Single().Target);
	}

	[Fact]
	public void FormState_Invalid_IsNotValidEvenWithoutErrorMessages()
	{
		var messages = new[] { MessageEntry.Warning("Optional date missing.", target: "Date") };
		var state = FormState.Invalid(messages);
		Assert.False(state.IsValid);
	}
}
