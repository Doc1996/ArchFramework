using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.Memory;

/// <summary>
/// Thread-safe in-memory notification store for development and tests.
/// </summary>
public sealed class MemoryNotificationStore : INotificationStore
{
	private readonly SyncDictionary<NotificationId, NotificationEntry> _entries = new();
	public IReadOnlyList<NotificationEntry> Entries => _entries.Values();

	public Task<Result> SaveAsync(NotificationEntry entry, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(entry);

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

	public void Clear() => _entries.Clear();
}
