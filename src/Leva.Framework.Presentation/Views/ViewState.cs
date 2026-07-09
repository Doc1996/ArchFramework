namespace Leva.Framework.Presentation;

/// <summary>
/// Stores the current presentation state for one screen, page, or section.
/// </summary>
public sealed record ViewState<TValue>(ViewStatus Status, TValue? Value = default, MessageEntry? Message = null)
{
	public bool IsLoading => Status == ViewStatus.Loading;
	public bool IsReady => Status == ViewStatus.Ready;
	public bool IsEmpty => Status == ViewStatus.Empty;
	public bool HasFailed => Status == ViewStatus.Failed;

	public static ViewState<TValue> Idle() => new(ViewStatus.Idle);

	public static ViewState<TValue> Loading(string? message = null) =>
		new(ViewStatus.Loading, Message: message is null ? null : MessageEntry.Info(message));

	public static ViewState<TValue> Ready(TValue value, string? message = null) =>
		new(ViewStatus.Ready, value, message is null ? null : MessageEntry.Success(message));

	public static ViewState<TValue> Empty(string? message = null) =>
		new(ViewStatus.Empty, Message: message is null ? null : MessageEntry.Info(message));

	public static ViewState<TValue> Failed(string message, string? code = null) =>
		new(ViewStatus.Failed, Message: MessageEntry.Error(message, code: code));

	public static ViewState<TValue> Failed(MessageEntry message) => new(ViewStatus.Failed, Message: message);
}
