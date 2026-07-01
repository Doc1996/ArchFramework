using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Sends notifications through a concrete delivery mechanism.
/// </summary>
public interface INotificationGateway
{
	Task<Result> SendAsync(Notification notification, CancellationToken token = default);
}
