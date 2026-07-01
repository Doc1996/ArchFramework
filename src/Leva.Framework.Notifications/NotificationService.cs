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
		var result = await SendNotificationAsync(notification, token);
		var entry = CreateEntry(notification, result);

		var saved = await _store.SaveAsync(entry, token);
		if (saved.IsFailure)
			return Result<NotificationEntry>.Fail(saved.Error);

		return result.IsSuccess ? Result<NotificationEntry>.Ok(entry) : Result<NotificationEntry>.Fail(result.Error);
	}

	private async Task<Result> SendNotificationAsync(Notification notification, CancellationToken token)
	{
		try
		{
			return await _gateway.SendAsync(notification, token);
		}
		catch (OperationCanceledException)
		{
			return Result.Fail(NotificationErrors.Cancelled("send"));
		}
	}

	private NotificationEntry CreateEntry(Notification notification, Result result)
	{
		var completedAt = _clock.UtcNow;
		return result.IsSuccess
			? NotificationEntry.Sent(notification, completedAt)
			: NotificationEntry.Failed(notification, completedAt, result.Error);
	}
}
