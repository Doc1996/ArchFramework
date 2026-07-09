using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class DialogTests
{
	[Fact]
	public void DialogRequest_WithoutCancelText_CannotCancel()
	{
		var request = new DialogRequest("Saved.", "Plan");
		Assert.False(request.CanCancel);
		Assert.Equal("OK", request.AcceptText);
	}

	[Fact]
	public void DialogRequest_WithCancelText_CanCancel()
	{
		var request = new DialogRequest("Delete plan?", "Delete", "Delete", "Cancel");

		Assert.True(request.CanCancel);
		Assert.Equal("Delete", request.AcceptText);
		Assert.Equal("Cancel", request.CancelText);
	}

	[Fact]
	public void DialogResult_CreatesAcceptedAndCancelledResults()
	{
		Assert.True(DialogResult.Accept().Accepted);
		Assert.False(DialogResult.Cancel().Accepted);
	}
}
