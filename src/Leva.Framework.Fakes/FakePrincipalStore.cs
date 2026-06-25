using Leva.Framework.Core;
using Leva.Framework.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// In-memory principal store fake for tests.
/// </summary>
public sealed class FakePrincipalStore : IPrincipalStore
{
	private readonly Dictionary<PrincipalId, Principal> _identities = [];
	private readonly Dictionary<string, PrincipalId> _names = new(StringComparer.OrdinalIgnoreCase);
	public IReadOnlyDictionary<PrincipalId, Principal> Identities => _identities;

	public void Add(Principal principal, string? name = null)
	{
		ArgumentNullException.ThrowIfNull(principal);
		_identities[principal.Id] = principal;

		if (!string.IsNullOrWhiteSpace(name))
			_names[name] = principal.Id;
	}

	public Task<Result<Principal?>> LoadAsync(PrincipalId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_identities.TryGetValue(id, out var principal);
		return Task.FromResult(Result<Principal?>.Ok(principal));
	}

	public Task<Result<Principal?>> FindByNameAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		Principal? principal = null;

		if (_names.TryGetValue(name, out var id))
			_identities.TryGetValue(id, out principal);
		return Task.FromResult(Result<Principal?>.Ok(principal));
	}
}
