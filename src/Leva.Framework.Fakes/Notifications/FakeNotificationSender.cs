using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable notification sender fake with captured sends and one-shot failure behavior.
/// </summary>
public sealed class FakeNotificationSender : INotificationSender
{
	private readonly Lock _lock = new();
	private readonly List<Notification> _sent = [];
	private Error? _nextError;

	public IReadOnlyList<Notification> Sent
	{
		get
		{
			lock (_lock)
				return _sent.ToArray();
		}
	}

	public void FailNext(Error error)
	{
		lock (_lock)
			_nextError = error;
	}

	public Task<Result> SendAsync(Notification notification, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(notification);

		lock (_lock)
		{
			if (_nextError is { } error)
			{
				_nextError = null;
				return Task.FromResult(Result.Fail(error));
			}

			_sent.Add(notification);
		}

		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
		{
			_sent.Clear();
			_nextError = null;
		}
	}
}
