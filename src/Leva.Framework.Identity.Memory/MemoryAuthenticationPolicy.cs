using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Authenticates configured in-memory principal names for demos, samples, and tests.
/// </summary>
public sealed class MemoryAuthenticationPolicy : AuthenticationPolicy
{
	private readonly Lock _lock = new();
	private readonly IPrincipalStore _principals;
	private readonly Dictionary<string, PrincipalId> _names = new(StringComparer.OrdinalIgnoreCase);

	public MemoryAuthenticationPolicy(IPrincipalStore principals, AuthenticationMethod? method = null)
	{
		ArgumentNullException.ThrowIfNull(principals);
		_principals = principals;
		Method = method ?? MemoryAuthenticationMethods.Principal;
	}

	public override AuthenticationMethod Method { get; }

	public IReadOnlyDictionary<string, PrincipalId> Names
	{
		get
		{
			lock (_lock)
				return _names.ToDictionary();
		}
	}

	public void Add(string name, PrincipalId principalId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		lock (_lock)
			_names[name] = principalId;
	}

	public bool Remove(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		lock (_lock)
			return _names.Remove(name);
	}

	public void Clear()
	{
		lock (_lock)
			_names.Clear();
	}

	public override async Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		if (string.IsNullOrWhiteSpace(request.Name))
			return Succeeded(AuthenticationResult.Failed("Principal name is required."));

		var principalId = FindPrincipalId(request.Name);
		if (!principalId.HasValue)
			return Succeeded(AuthenticationResult.Failed("Principal name is invalid."));

		var principal = await _principals.LoadAsync(principalId.Value, token);
		if (principal.IsFailure)
			return Result<AuthenticationResult>.Fail(principal.Error);

		return principal.Value is null
			? Succeeded(AuthenticationResult.Failed("Principal was not found."))
			: Succeeded(AuthenticationResult.Succeeded(principal.Value));
	}

	private PrincipalId? FindPrincipalId(string name)
	{
		lock (_lock)
			return _names.TryGetValue(name, out var principalId) ? principalId : null;
	}

	private static Result<AuthenticationResult> Succeeded(AuthenticationResult result) =>
		Result<AuthenticationResult>.Ok(result);
}
