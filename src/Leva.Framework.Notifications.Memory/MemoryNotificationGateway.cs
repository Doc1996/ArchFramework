using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.Memory;

/// <summary>
/// In-memory gateway that records sent notifications.
/// </summary>
public sealed class MemoryNotificationGateway : INotificationGateway
{
	private readonly SyncList<Notification> _sent = new();
	public IReadOnlyList<Notification> Sent => _sent.List();

	public Task<Result> SendAsync(Notification notification, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(notification);

		_sent.Add(notification);
		return Task.FromResult(Result.Ok());
	}

	public void Clear() => _sent.Clear();
}
