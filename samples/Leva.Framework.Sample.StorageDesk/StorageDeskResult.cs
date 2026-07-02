namespace Leva.Framework.Sample.StorageDesk;

internal sealed record StorageDeskResult(
	bool Passed,
	int NotificationsSent,
	int NotificationEntriesStored,
	IReadOnlyList<StorageDeskScenarioResult> Scenarios,
	IReadOnlyList<StorageDeskCheck> Checks
);
