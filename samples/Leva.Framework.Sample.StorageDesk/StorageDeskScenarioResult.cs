namespace Leva.Framework.Sample.StorageDesk;

internal sealed record StorageDeskScenarioResult(
	string Provider,
	bool Passed,
	string Message,
	string TicketTitle,
	string AssignedTo,
	string Status,
	int ActivityEntries,
	int NotificationsSent,
	int NotificationEntriesStored
)
{
	public static StorageDeskScenarioResult Failed(string provider, string message) =>
		new(provider, false, message, string.Empty, string.Empty, string.Empty, 0, 0, 0);
}
