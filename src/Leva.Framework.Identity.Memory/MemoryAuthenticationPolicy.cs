using Leva.Framework.Core;

namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Authenticates principals from in-memory name and secret credentials.
/// </summary>
public sealed class MemoryAuthenticationPolicy : AuthenticationPolicy
{
	private readonly Lock _lock = new();
	private readonly IPrincipalStore _principals;
	private readonly Dictionary<string, MemoryPrincipalCredential> _credentials = new(StringComparer.OrdinalIgnoreCase);

	public MemoryAuthenticationPolicy(IPrincipalStore principals, AuthenticationMethod? method = null)
	{
		ArgumentNullException.ThrowIfNull(principals);
		_principals = principals;
		Method = method ?? MemoryAuthenticationMethods.Secret;
	}

	public override AuthenticationMethod Method { get; }

	public IReadOnlyList<MemoryPrincipalCredential> Credentials
	{
		get
		{
			lock (_lock)
				return _credentials.Values.ToList();
		}
	}

	public void Add(MemoryPrincipalCredential credential)
	{
		ArgumentNullException.ThrowIfNull(credential);
		lock (_lock)
			_credentials[credential.Name] = credential;
	}

	public void Add(string name, string secret, PrincipalId principalId) =>
		Add(new MemoryPrincipalCredential(name, secret, principalId));

	public bool Remove(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		lock (_lock)
			return _credentials.Remove(name);
	}

	public void Clear()
	{
		lock (_lock)
			_credentials.Clear();
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

		if (string.IsNullOrWhiteSpace(request.Secret))
			return Succeeded(AuthenticationResult.Failed("Principal secret is required."));

		var credential = FindCredential(request.Name);
		if (credential is null || credential.Secret != request.Secret)
			return Succeeded(AuthenticationResult.Failed("Principal credentials are invalid."));

		var principal = await _principals.LoadAsync(credential.PrincipalId, token);
		if (principal.IsFailure)
			return Result<AuthenticationResult>.Fail(principal.Error);

		return principal.Value is null
			? Succeeded(AuthenticationResult.Failed("Principal was not found."))
			: Succeeded(AuthenticationResult.Succeeded(principal.Value));
	}

	private MemoryPrincipalCredential? FindCredential(string name)
	{
		lock (_lock)
			return _credentials.TryGetValue(name, out var credential) ? credential : null;
	}

	private static Result<AuthenticationResult> Succeeded(AuthenticationResult result) =>
		Result<AuthenticationResult>.Ok(result);
}
