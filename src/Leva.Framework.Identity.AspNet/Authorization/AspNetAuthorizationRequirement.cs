using Microsoft.AspNetCore.Authorization;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Wraps a framework authorization requirement for ASP.NET Core authorization policies.
/// </summary>
public sealed class AspNetAuthorizationRequirement(AuthorizationRequirement requirement) : IAuthorizationRequirement
{
	public AuthorizationRequirement Requirement { get; } = requirement;
}
