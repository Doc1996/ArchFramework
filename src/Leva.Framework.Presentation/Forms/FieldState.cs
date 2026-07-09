namespace Leva.Framework.Presentation;

/// <summary>
/// Stores the current presentation state for one user-facing form field.
/// </summary>
public sealed record FieldState<TValue>(
	TValue Value,
	bool IsTouched = false,
	bool IsModified = false,
	IReadOnlyList<MessageEntry>? Messages = null
)
{
	public bool IsValid => Messages is null || !Messages.Any(message => message.Level == MessageLevel.Error);

	public FieldState<TValue> WithValue(TValue value) => this with { Value = value, IsModified = true };

	public FieldState<TValue> Touched() => this with { IsTouched = true };

	public FieldState<TValue> Failed(string message, string? code = null, string? target = null) =>
		this with
		{
			Messages = [MessageEntry.Error(message, code: code, target: target)],
		};
}
