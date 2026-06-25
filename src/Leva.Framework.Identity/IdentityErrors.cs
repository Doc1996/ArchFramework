using Leva.Framework.Core;

namespace Leva.Framework.Identity;

/// <summary>
/// Creates common structured errors for identity contracts and providers.
/// </summary>
public static class IdentityErrors
{
	public static Error NotFound(string resource) =>
		new("identity.not_found", $"Identity resource '{resource}' was not found.");

	public static Error NotFound(string resource, string key) =>
		new("identity.not_found", $"Identity resource '{resource}' with key '{key}' was not found.");

	public static Error Conflict(string resource, string? details = null) =>
		new(
			"identity.conflict",
			WithDetails($"Identity resource '{resource}' already exists or has changed.", details)
		);

	public static Error Invalid(string resource, string? details = null) =>
		new("identity.invalid", WithDetails($"Identity resource '{resource}' is invalid.", details));

	public static Error Unavailable(string provider, string? details = null) =>
		new("identity.unavailable", WithDetails($"Identity provider '{provider}' is unavailable.", details));

	public static Error Failed(string operation, string? details = null) =>
		new("identity.failed", WithDetails($"Identity operation '{operation}' failed.", details));

	public static Error Unauthorized(string? details = null) =>
		new("identity.unauthorized", WithDetails("Identity is not authenticated.", details));

	public static Error Forbidden(string? details = null) =>
		new("identity.forbidden", WithDetails("Identity is not authorized.", details));

	private static string WithDetails(string message, string? details) =>
		string.IsNullOrWhiteSpace(details) ? message : $"{message} {details}";
}
