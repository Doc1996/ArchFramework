using Leva.Framework.Core;

namespace Leva.Framework.Presentation;

/// <summary>
/// Represents one user-facing presentation message for views, commands, forms, and transient feedback.
/// </summary>
public sealed record MessageEntry(
	string Text,
	MessageLevel Level = MessageLevel.Info,
	string? Title = null,
	string? Code = null,
	string? Target = null
)
{
	public static MessageEntry Info(string text, string? title = null, string? target = null) =>
		new(text, MessageLevel.Info, title, Target: target);

	public static MessageEntry Success(string text, string? title = null, string? target = null) =>
		new(text, MessageLevel.Success, title, Target: target);

	public static MessageEntry Warning(string text, string? title = null, string? code = null, string? target = null) =>
		new(text, MessageLevel.Warning, title, code, target);

	public static MessageEntry Error(string text, string? title = null, string? code = null, string? target = null) =>
		new(text, MessageLevel.Error, title, code, target);

	public static MessageEntry FromError(Error error, string? title = null, string? target = null) =>
		new(error.Message, MessageLevel.Error, title, error.Code, target);
}
