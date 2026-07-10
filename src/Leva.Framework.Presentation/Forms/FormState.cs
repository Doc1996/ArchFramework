namespace Leva.Framework.Presentation;

/// <summary>
/// Stores the current presentation state for one user-facing form.
/// </summary>
public sealed record FormState(FormStatus Status, IReadOnlyList<MessageEntry>? Messages = null)
{
	public bool CanSubmit => IsValid && (Status is FormStatus.Clean or FormStatus.Modified);
	public bool IsSubmitting => Status == FormStatus.Submitting;
	public bool IsValid =>
		Status != FormStatus.Invalid
		&& (Messages is null || !Messages.Any(message => message.Level == MessageLevel.Error));

	public static FormState Clean() => new(FormStatus.Clean);

	public static FormState Modified() => new(FormStatus.Modified);

	public static FormState Submitting() => new(FormStatus.Submitting);

	public static FormState Submitted(string? message = null) =>
		new(FormStatus.Submitted, message is null ? null : [MessageEntry.Success(message)]);

	public static FormState Invalid(IReadOnlyList<MessageEntry> messages) => new(FormStatus.Invalid, messages);

	public static FormState Failed(string message, string? code = null) =>
		new(FormStatus.Failed, [MessageEntry.Error(message, code: code)]);

	public static FormState Failed(MessageEntry message) => new(FormStatus.Failed, [message]);
}
