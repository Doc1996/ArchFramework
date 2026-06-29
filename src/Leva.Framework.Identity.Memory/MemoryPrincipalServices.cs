namespace Leva.Framework.Identity.Memory;

/// <summary>
/// Groups common in-memory principal services for demos, samples, and tests.
/// </summary>
public sealed class MemoryPrincipalServices
{
	private MemoryPrincipalServices(
		MemoryPrincipalStore principals,
		MemoryAuthSessionStore sessions,
		AuthSessionService sessionService,
		AuthenticationService authentication,
		AuthorizationService authorization,
		MemoryAuthSessionSource sessionSource,
		PrincipalAccess access
	)
	{
		Principals = principals;
		Sessions = sessions;
		SessionService = sessionService;
		Authentication = authentication;
		Authorization = authorization;
		SessionSource = sessionSource;
		Access = access;
	}

	public MemoryPrincipalStore Principals { get; }
	public MemoryAuthSessionStore Sessions { get; }
	public AuthSessionService SessionService { get; }
	public AuthenticationService Authentication { get; }
	public AuthorizationService Authorization { get; }
	public MemoryAuthSessionSource SessionSource { get; }
	public PrincipalAccess Access { get; }

	public static MemoryPrincipalServices Create(IAuditSink? auditSink = null, AuthSessionPolicy? sessionPolicy = null)
	{
		var principals = new MemoryPrincipalStore();
		var sessions = new MemoryAuthSessionStore();
		var sessionService = new AuthSessionService(sessions, sessionPolicy, auditSink);
		var authentication = new AuthenticationService(
			[new MemoryAuthenticationPolicy(principals)],
			sessionService,
			auditSink
		);
		var authorization = new AuthorizationService([new BuiltInAuthorizationPolicy()], auditSink);
		var sessionSource = new MemoryAuthSessionSource(sessionService);
		var access = new PrincipalAccess(sessionSource, authorization);

		return new MemoryPrincipalServices(
			principals,
			sessions,
			sessionService,
			authentication,
			authorization,
			sessionSource,
			access
		);
	}
}
