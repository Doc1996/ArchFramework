namespace Leva.Framework.Presentation;

/// <summary>
/// Receives transient user-facing presentation messages.
/// </summary>
public interface IMessageSink
{
	void Write(MessageEntry message);
}
