using Leva.Framework.Core;
using Leva.Framework.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// Sends notifications to connected SignalR users.
/// </summary>
public sealed class SignalRNotificationGateway : INotificationGateway
{
	private readonly IHubContext<SignalRNotificationHub, ISignalRNotificationClient> _hub;
	private readonly SignalRNotificationOptions _options;

	public SignalRNotificationGateway(
		IHubContext<SignalRNotificationHub, ISignalRNotificationClient> hub,
		IOptions<SignalRNotificationOptions> options
	)
	{
		ArgumentNullException.ThrowIfNull(hub);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.Channel.Value);

		_hub = hub;
		_options = options.Value;
	}

	public async Task<Result> SendAsync(Notification notification, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(notification);

		if (notification.Channel != _options.Channel)
			return Result.Fail(NotificationErrors.UnsupportedChannel(notification.Channel));

		if (string.IsNullOrWhiteSpace(notification.Recipient.Id))
			return Result.Fail(
				NotificationErrors.Failed("signalr", "Recipient id is required for SignalR user targeting.")
			);

		try
		{
			var payload = SignalRNotificationPayload.From(notification);
			await _hub.Clients.User(notification.Recipient.Id).ReceiveNotification(payload);
			return Result.Ok();
		}
		catch (OperationCanceledException)
		{
			return Result.Fail(NotificationErrors.Cancelled("signalr"));
		}
		catch (Exception exception)
		{
			return Result.Fail(NotificationErrors.Failed("signalr", exception.Message));
		}
	}
}
