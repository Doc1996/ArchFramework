using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable notification store for tests.
/// </summary>
public sealed class FakeNotificationStore : INotificationStore
{
	private readonly Lock _lock = new();
	private readonly Dictionary<NotificationId, NotificationEntry> _entries = [];
	private Error? _nextSaveError;

	public IReadOnlyList<NotificationEntry> Entries
	{
		get
		{
			lock (_lock)
				return _entries.Values.ToArray();
		}
	}

	public void FailNextSave(Error error)
	{
		lock (_lock)
			_nextSaveError = error;
	}

	public Task<Result> SaveAsync(NotificationEntry entry, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(entry);
		Error? error;

		lock (_lock)
		{
			error = _nextSaveError;
			_nextSaveError = null;

			if (error is null)
				_entries[entry.NotificationId] = entry;
		}

		return Task.FromResult(error is null ? Result.Ok() : Result.Fail(error));
	}

	public Task<Result<NotificationEntry?>> LoadAsync(NotificationId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
			return Task.FromResult(Result<NotificationEntry?>.Ok(_entries.GetValueOrDefault(id)));
	}

	public Task<Result<IReadOnlyList<NotificationEntry>>> LoadAllAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
			return Task.FromResult(Result<IReadOnlyList<NotificationEntry>>.Ok(_entries.Values.ToArray()));
	}

	public Task<Result> DeleteAsync(NotificationId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
			_entries.Remove(id);

		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
		{
			_entries.Clear();
			_nextSaveError = null;
		}
	}
}
