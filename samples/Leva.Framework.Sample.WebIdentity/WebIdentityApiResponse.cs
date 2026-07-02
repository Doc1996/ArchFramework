namespace Leva.Framework.Sample.WebIdentity;

internal sealed record WebIdentityApiResponse(
	string Message,
	string? Name,
	bool IsAuthenticated,
	IReadOnlyList<string> Roles,
	IReadOnlyList<string> Permissions
);
