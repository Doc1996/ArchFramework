using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Sends notifications through one or more delivery channels.
/// </summary>
public interface INotificationSender
{
	Task<Result> SendAsync(Notification notification, CancellationToken token = default);
}
