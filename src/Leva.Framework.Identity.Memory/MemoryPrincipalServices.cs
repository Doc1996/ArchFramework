namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Groups common in-memory principal services for demos, samples, and tests.
/// </summary>
public sealed class MemoryPrincipalServices(
	MemoryPrincipalStore principals,
	MemoryPrincipalSessionStore sessions,
	MemoryAuthenticationPolicy authenticationPolicy,
	MemoryAuthorizationPolicy authorizationPolicy,
	PrincipalSessionService sessionService,
	AuthenticationService authentication,
	AuthorizationService authorization,
	MemoryPrincipalSessionSource sessionSource,
	PrincipalAccess access
)
{
	public MemoryPrincipalStore Principals { get; } = principals;
	public MemoryPrincipalSessionStore Sessions { get; } = sessions;
	public MemoryAuthenticationPolicy AuthenticationPolicy { get; } = authenticationPolicy;
	public MemoryAuthorizationPolicy AuthorizationPolicy { get; } = authorizationPolicy;
	public PrincipalSessionService SessionService { get; } = sessionService;
	public AuthenticationService Authentication { get; } = authentication;
	public AuthorizationService Authorization { get; } = authorization;
	public MemoryPrincipalSessionSource SessionSource { get; } = sessionSource;
	public PrincipalAccess Access { get; } = access;

	public static MemoryPrincipalServices Create(
		IAuditSink? auditSink = null,
		PrincipalSessionPolicy? sessionPolicy = null
	)
	{
		var principals = new MemoryPrincipalStore();
		var sessions = new MemoryPrincipalSessionStore();
		var authenticationPolicy = new MemoryAuthenticationPolicy(principals);
		var authorizationPolicy = new MemoryAuthorizationPolicy();

		var sessionService = new PrincipalSessionService(sessions, sessionPolicy, auditSink);
		var authentication = new AuthenticationService([authenticationPolicy], sessionService, auditSink);
		var authorization = new AuthorizationService([authorizationPolicy], auditSink);
		var sessionSource = new MemoryPrincipalSessionSource(sessionService);
		var access = new PrincipalAccess(sessionSource, authorization);

		return new MemoryPrincipalServices(
			principals,
			sessions,
			authenticationPolicy,
			authorizationPolicy,
			sessionService,
			authentication,
			authorization,
			sessionSource,
			access
		);
	}
}
