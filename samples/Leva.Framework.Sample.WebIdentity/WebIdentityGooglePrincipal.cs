namespace Leva.Framework.Sample.WebIdentity;

internal sealed record WebIdentityGooglePrincipal(
	bool IsAuthenticated,
	string? Name,
	string? Email,
	string[] Roles,
	string[] Permissions
);
