using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable notification store for tests.
/// </summary>
public sealed class FakeNotificationStore : INotificationStore
{
	private readonly Lock _lock = new();
	private readonly SyncDictionary<NotificationId, NotificationEntry> _entries = new();
	private Error? _nextSaveError;

	public IReadOnlyList<NotificationEntry> Entries => _entries.Values();

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
		}

		if (error is not null)
			return Task.FromResult(Result.Fail(error.Value));

		_entries.Set(entry.NotificationId, entry);
		return Task.FromResult(Result.Ok());
	}

	public Task<Result<NotificationEntry?>> LoadAsync(NotificationId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(Result<NotificationEntry?>.Ok(_entries.GetOrDefault(id)));
	}

	public Task<Result<IReadOnlyList<NotificationEntry>>> LoadAllAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(Result<IReadOnlyList<NotificationEntry>>.Ok(_entries.Values()));
	}

	public Task<Result> DeleteAsync(NotificationId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_entries.Remove(id);

		return Task.FromResult(Result.Ok());
	}

	public void Clear()
	{
		lock (_lock)
			_nextSaveError = null;

		_entries.Clear();
	}
}
