using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.Memory;

/// <summary>
/// Thread-safe in-memory notification store for development and tests.
/// </summary>
public sealed class MemoryNotificationStore : INotificationStore
{
	private readonly Lock _lock = new();
	private readonly Dictionary<NotificationId, NotificationEntry> _entries = [];

	public IReadOnlyList<NotificationEntry> Entries
	{
		get
		{
			lock (_lock)
				return _entries.Values.ToArray();
		}
	}

	public Task<Result> SaveAsync(NotificationEntry entry, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(entry);

		lock (_lock)
			_entries[entry.NotificationId] = entry;

		return Task.FromResult(Result.Ok());
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
			_entries.Clear();
	}
}
