using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Stores notification entries without exposing provider-specific persistence details.
/// </summary>
public interface INotificationStore
{
	Task<Result> SaveAsync(NotificationEntry entry, CancellationToken token = default);

	Task<Result<NotificationEntry?>> LoadAsync(NotificationId id, CancellationToken token = default);
	Task<Result<IReadOnlyList<NotificationEntry>>> LoadAllAsync(CancellationToken token = default);

	Task<Result> DeleteAsync(NotificationId id, CancellationToken token = default);
}
