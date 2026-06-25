using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Creates common structured errors for principal contracts and providers.
/// </summary>
public static class PrincipalErrors
{
	public static Error NotFound(string resource) =>
		new("principal.not_found", $"Principal resource '{resource}' was not found.");

	public static Error NotFound(string resource, string key) =>
		new("principal.not_found", $"Principal resource '{resource}' with key '{key}' was not found.");

	public static Error Conflict(string resource, string? details = null) =>
		new(
			"principal.conflict",
			WithDetails($"Principal resource '{resource}' already exists or has changed.", details)
		);

	public static Error Invalid(string resource, string? details = null) =>
		new("principal.invalid", WithDetails($"Principal resource '{resource}' is invalid.", details));

	public static Error Unavailable(string provider, string? details = null) =>
		new("principal.unavailable", WithDetails($"Principal provider '{provider}' is unavailable.", details));

	public static Error Failed(string operation, string? details = null) =>
		new("principal.failed", WithDetails($"Principal operation '{operation}' failed.", details));

	public static Error Unauthorized(string? details = null) =>
		new("principal.unauthorized", WithDetails("Principal is not authenticated.", details));

	public static Error Forbidden(string? details = null) =>
		new("principal.forbidden", WithDetails("Principal is not authorized.", details));

	private static string WithDetails(string message, string? details) =>
		string.IsNullOrWhiteSpace(details) ? message : $"{message} {details}";
}
