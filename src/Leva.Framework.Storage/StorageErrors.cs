using Leva.Framework.Core;

namespace Leva.Framework.Storage;

/// <summary>
/// Creates common structured errors for storage contracts and providers.
/// </summary>
public static class StorageErrors
{
	public static Error NotFound(string resource, string key) =>
		new("storage.not_found", $"Storage resource '{resource}' with key '{key}' was not found.");

	public static Error VersionConflict(string resource, string key, StorageVersion expected, StorageVersion actual) =>
		new(
			"storage.version_conflict",
			$"Storage resource '{resource}' with key '{key}' has version '{actual.Value}', but version '{expected.Value}' was expected."
		);

	public static Error Unavailable(string provider, string? details = null) =>
		new("storage.unavailable", WithDetails($"Storage provider '{provider}' is unavailable.", details));

	public static Error Failed(string operation, string? details = null) =>
		new("storage.failed", WithDetails($"Storage operation '{operation}' failed.", details));

	private static string WithDetails(string message, string? details) =>
		string.IsNullOrWhiteSpace(details) ? message : $"{message} {details}";
}
