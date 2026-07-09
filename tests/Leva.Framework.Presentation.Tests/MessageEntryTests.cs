using Leva.Framework.Core;
using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class MessageEntryTests
{
	[Fact]
	public void MessageEntry_CreatesInfoMessage()
	{
		var message = MessageEntry.Info("Loaded.", "Plan", "header");
		Assert.Equal("Loaded.", message.Text);
		Assert.Equal(MessageLevel.Info, message.Level);

		Assert.Equal("Plan", message.Title);
		Assert.Equal("header", message.Target);
		Assert.Null(message.Code);
	}

	[Fact]
	public void MessageEntry_CreatesSuccessMessage()
	{
		var message = MessageEntry.Success("Saved.");
		Assert.Equal("Saved.", message.Text);
		Assert.Equal(MessageLevel.Success, message.Level);
	}

	[Fact]
	public void MessageEntry_CreatesWarningMessage()
	{
		var message = MessageEntry.Warning("Incomplete.", code: "plan.incomplete", target: "participants");

		Assert.Equal(MessageLevel.Warning, message.Level);
		Assert.Equal("plan.incomplete", message.Code);
		Assert.Equal("participants", message.Target);
	}

	[Fact]
	public void MessageEntry_CreatesErrorMessage()
	{
		var message = MessageEntry.Error("Could not save.", code: "plan.save_failed");
		Assert.Equal(MessageLevel.Error, message.Level);
		Assert.Equal("plan.save_failed", message.Code);
	}

	[Fact]
	public void MessageEntry_CreatesMessageFromCoreError()
	{
		var error = new Error("storage.unavailable", "Storage is unavailable.");
		var message = MessageEntry.FromError(error, "Storage", "plan");

		Assert.Equal("Storage is unavailable.", message.Text);
		Assert.Equal(MessageLevel.Error, message.Level);
		Assert.Equal("Storage", message.Title);

		Assert.Equal("storage.unavailable", message.Code);
		Assert.Equal("plan", message.Target);
	}
}
