using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Creates notifications, sends them through a gateway, and stores notification entries.
/// </summary>
public sealed class NotificationService
{
	private readonly INotificationGateway _gateway;
	private readonly INotificationStore _store;
	private readonly IClock _clock;

	public NotificationService(INotificationGateway gateway, INotificationStore store, IClock clock)
	{
		ArgumentNullException.ThrowIfNull(gateway);
		ArgumentNullException.ThrowIfNull(store);
		ArgumentNullException.ThrowIfNull(clock);

		_gateway = gateway;
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
		ArgumentException.ThrowIfNullOrWhiteSpace(channel.Value);
		ArgumentException.ThrowIfNullOrWhiteSpace(subject);
		ArgumentException.ThrowIfNullOrWhiteSpace(body);

		var notification = new Notification(NotificationId.New(), recipient, channel, subject, body, _clock.UtcNow);
		try
		{
			var result = await _gateway.SendAsync(notification, token);
			var entry = result.IsSuccess
				? NotificationEntry.Sent(notification, _clock.UtcNow)
				: NotificationEntry.Failed(notification, _clock.UtcNow, result.Error);

			var saved = await _store.SaveAsync(entry, token);
			if (saved.IsFailure)
				return Result<NotificationEntry>.Fail(saved.Error);

			return result.IsSuccess
				? Result<NotificationEntry>.Ok(entry)
				: Result<NotificationEntry>.Fail(result.Error);
		}
		catch (OperationCanceledException)
		{
			var entry = NotificationEntry.Cancelled(
				notification,
				_clock.UtcNow,
				NotificationErrors.Failed("send", "Notification send was cancelled.")
			);

			await _store.SaveAsync(entry, CancellationToken.None);
			return Result<NotificationEntry>.Fail(entry.Error!);
		}
	}
}
