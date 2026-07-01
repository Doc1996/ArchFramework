using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.Memory;

/// <summary>
/// In-memory sender that records sent notifications.
/// </summary>
public sealed class MemoryNotificationSender : INotificationSender
{
	private readonly Lock _lock = new();
	private readonly List<Notification> _sent = [];

	public IReadOnlyList<Notification> Sent
	{
		get
		{
			lock (_lock)
				return _sent.ToArray();
		}
	}

	public Task<Result> SendAsync(Notification notification, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(notification);

		lock (_lock)
			_sent.Add(notification);

		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
			_sent.Clear();
	}
}
