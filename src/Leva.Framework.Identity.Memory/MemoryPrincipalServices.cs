namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Groups common in-memory principal services for demos, samples, and tests.
/// </summary>
public sealed class MemoryPrincipalServices(
	MemoryPrincipalStore principals,
	MemoryAuthSessionStore sessions,
	MemoryAuthenticationPolicy authenticationPolicy,
	BuiltInAuthorizationPolicy authorizationPolicy,
	AuthSessionService sessionService,
	AuthenticationService authentication,
	AuthorizationService authorization,
	MemoryAuthSessionSource sessionSource,
	PrincipalAccess access
)
{
	public MemoryPrincipalStore Principals { get; } = principals;
	public MemoryAuthSessionStore Sessions { get; } = sessions;
	public MemoryAuthenticationPolicy AuthenticationPolicy { get; } = authenticationPolicy;
	public BuiltInAuthorizationPolicy AuthorizationPolicy { get; } = authorizationPolicy;
	public AuthSessionService SessionService { get; } = sessionService;
	public AuthenticationService Authentication { get; } = authentication;
	public AuthorizationService Authorization { get; } = authorization;
	public MemoryAuthSessionSource SessionSource { get; } = sessionSource;
	public PrincipalAccess Access { get; } = access;

	public static MemoryPrincipalServices Create(IAuditSink? auditSink = null, AuthSessionPolicy? sessionPolicy = null)
	{
		var principals = new MemoryPrincipalStore();
		var sessions = new MemoryAuthSessionStore();
		var authenticationPolicy = new MemoryAuthenticationPolicy(principals);
		var authorizationPolicy = new BuiltInAuthorizationPolicy();

		var sessionService = new AuthSessionService(sessions, sessionPolicy, auditSink);
		var authentication = new AuthenticationService([authenticationPolicy], sessionService, auditSink);
		var authorization = new AuthorizationService([authorizationPolicy], auditSink);
		var sessionSource = new MemoryAuthSessionSource(sessionService);
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
