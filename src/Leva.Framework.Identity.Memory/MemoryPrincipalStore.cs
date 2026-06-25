using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Stores principals in memory and resolves them by identifier or configured login name.
/// </summary>
public sealed class MemoryPrincipalStore : IPrincipalStore
{
	private readonly Lock _lock = new();
	private readonly Dictionary<PrincipalId, Principal> _principals = [];
	private readonly Dictionary<string, PrincipalId> _names = new(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyList<Principal> Principals
	{
		get
		{
			lock (_lock)
				return _principals.Values.ToList();
		}
	}

	public void Add(Principal principal, params string[] names)
	{
		ArgumentNullException.ThrowIfNull(principal);
		lock (_lock)
		{
			_principals[principal.Id] = principal;
			AddName(principal.DisplayName, principal.Id);

			if (!string.IsNullOrWhiteSpace(principal.Email))
				AddName(principal.Email, principal.Id);

			foreach (var name in names)
				AddName(name, principal.Id);
		}
	}

	public bool Remove(PrincipalId id)
	{
		lock (_lock)
		{
			if (!_principals.Remove(id))
				return false;

			foreach (var name in _names.Where(pair => pair.Value == id).Select(pair => pair.Key).ToList())
				_names.Remove(name);
			return true;
		}
	}

	public void Clear()
	{
		lock (_lock)
		{
			_principals.Clear();
			_names.Clear();
		}
	}

	public Task<Result<Principal?>> LoadAsync(PrincipalId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		lock (_lock)
		{
			_principals.TryGetValue(id, out var principal);
			return Task.FromResult(Result<Principal?>.Ok(principal));
		}
	}

	public Task<Result<Principal?>> FindByNameAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		lock (_lock)
			return Task.FromResult(Result<Principal?>.Ok(FindByName(name)));
	}

	private Principal? FindByName(string name)
	{
		if (!_names.TryGetValue(name, out var id))
			return null;
		return _principals.TryGetValue(id, out var principal) ? principal : null;
	}

	private void AddName(string name, PrincipalId id)
	{
		if (!string.IsNullOrWhiteSpace(name))
			_names[name] = id;
	}
}
