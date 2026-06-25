using Leva.Framework.Core;
using Leva.Framework.Identity;
using FrameworkIdentity = Leva.Framework.Identity.Identity;

namespace Leva.Framework.Fakes;

/// <summary>
/// In-memory identity store fake for tests.
/// </summary>
public sealed class FakeIdentityStore : IIdentityStore
{
	private readonly Dictionary<IdentityId, FrameworkIdentity> _identities = [];
	private readonly Dictionary<string, IdentityId> _names = new(StringComparer.OrdinalIgnoreCase);
	public IReadOnlyDictionary<IdentityId, FrameworkIdentity> Identities => _identities;

	public void Add(FrameworkIdentity identity, string? name = null)
	{
		ArgumentNullException.ThrowIfNull(identity);
		_identities[identity.Id] = identity;

		if (!string.IsNullOrWhiteSpace(name))
			_names[name] = identity.Id;
	}

	public Task<Result<FrameworkIdentity?>> LoadAsync(IdentityId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_identities.TryGetValue(id, out var identity);
		return Task.FromResult(Result<FrameworkIdentity?>.Ok(identity));
	}

	public Task<Result<FrameworkIdentity?>> FindByNameAsync(string name, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return Task.FromResult(
			Result<FrameworkIdentity?>.Ok(
				_names.TryGetValue(name, out var id) && _identities.TryGetValue(id, out var identity) ? identity : null
			)
		);
	}
}
