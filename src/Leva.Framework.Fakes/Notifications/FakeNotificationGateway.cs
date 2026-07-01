using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable notification gateway fake with captured sends and one-shot failure behavior.
/// </summary>
public sealed class FakeNotificationGateway : INotificationGateway
{
	private readonly Lock _lock = new();
	private readonly SyncList<Notification> _sent = new();
	private Error? _nextError;
	private bool _cancelNext;

	public IReadOnlyList<Notification> Sent => _sent.List();

	public void FailNext(Error error)
	{
		lock (_lock)
		{
			_nextError = error;
			_cancelNext = false;
		}
	}

	public void CancelNext()
	{
		lock (_lock)
		{
			_nextError = null;
			_cancelNext = true;
		}
	}

	public Task<Result> SendAsync(Notification notification, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(notification);

		lock (_lock)
		{
			if (_cancelNext)
			{
				_cancelNext = false;
				throw new OperationCanceledException();
			}

			if (_nextError is { } error)
			{
				_nextError = null;
				return Task.FromResult(Result.Fail(error));
			}
		}

		_sent.Add(notification);
		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
		{
			_nextError = null;
			_cancelNext = false;
		}

		_sent.Clear();
	}
}
