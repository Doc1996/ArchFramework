namespace Leva.Framework.Notifications;

/// <summary>
/// Identifies the delivery channel used by a notification.
/// </summary>
public readonly record struct NotificationChannel
{
	public NotificationChannel(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		Value = value;
	}

	public string Value { get; } = string.Empty;

	public override string ToString() => Value;
}
