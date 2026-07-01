using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Creates notifications, sends them, and stores notification entries.
/// </summary>
public sealed class NotificationService
{
	private readonly INotificationSender _sender;
	private readonly INotificationStore _store;
	private readonly IClock _clock;

	public NotificationService(INotificationSender sender, INotificationStore store, IClock clock)
	{
		ArgumentNullException.ThrowIfNull(sender);
		ArgumentNullException.ThrowIfNull(store);
		ArgumentNullException.ThrowIfNull(clock);

		_sender = sender;
		_store = store;
		_clock = clock;
	}

	public async Task<Result<NotificationEntry>> SendAsync(
		NotificationRecipient recipient,
		NotificationChannel channel,
		string subject,
		string body,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();

		ArgumentNullException.ThrowIfNull(recipient);
		ArgumentException.ThrowIfNullOrWhiteSpace(subject);
		ArgumentException.ThrowIfNullOrWhiteSpace(body);

		var notification = new Notification(NotificationId.New(), recipient, channel, subject, body, _clock.UtcNow);
		var result = await _sender.SendAsync(notification, token);

		var entry = result.IsSuccess
			? NotificationEntry.Sent(notification, _clock.UtcNow)
			: NotificationEntry.Failed(notification, _clock.UtcNow, result.Error);

		var saved = await _store.SaveAsync(entry, token);
		if (saved.IsFailure)
			return Result<NotificationEntry>.Fail(saved.Error);

		return result.IsSuccess ? Result<NotificationEntry>.Ok(entry) : Result<NotificationEntry>.Fail(result.Error);
	}
}
