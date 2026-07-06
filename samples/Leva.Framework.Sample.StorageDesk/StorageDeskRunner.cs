using Leva.Framework.Core;
using Leva.Framework.Identity;
using Leva.Framework.Identity.Memory;
using Leva.Framework.Notifications.Memory;
using Leva.Framework.Storage;
using Leva.Framework.Storage.Files;
using Leva.Framework.Storage.Memory;
using Leva.Framework.Storage.Sqlite;

namespace Leva.Framework.Sample.StorageDesk;

internal static class StorageDeskRunner
{
	private static readonly PrincipalPermission ManagerPermission = new("tickets.manage");

	public static async Task<StorageDeskResult> RunAsync(CancellationToken token = default)
	{
		// One clock is shared by storage models and notification entries so timestamps are consistent.
		var clock = new SystemClock();
		// Memory identity provides principals, sessions, authentication, authorization, and current access for the sample.
		var identity = MemoryPrincipalServices.Create();
		// Memory notifications give both a gateway and a store so the page can verify sent and stored entries.
		var notifications = MemoryNotificationServices.Create(clock);
		var login = await CreateAndLoginManagerAsync(identity, token);

		// The same application flow receives provider-neutral IRepository and IJournal contracts.
		// Only the composition code below chooses memory, file, or SQLite persistence.
		var scenarios = new[]
		{
			await RunMemoryScenarioAsync(identity, notifications, clock, token),
			await RunFileScenarioAsync(identity, notifications, clock, token),
			await RunSqliteScenarioAsync(identity, notifications, clock, token),
		};

		var checks = new[]
		{
			Check("identity login created an active session", login.IsSuccess && login.Value?.Session is not null),
			Check("all storage providers completed the same scenario", scenarios.All(scenario => scenario.Passed)),
			Check("two notifications were sent per scenario", notifications.Gateway.Sent.Count == scenarios.Length * 2),
			Check(
				"two notification entries were stored per scenario",
				notifications.Store.Entries.Count == scenarios.Length * 2
			),
		};

		return new StorageDeskResult(
			checks.All(check => check.Passed),
			notifications.Gateway.Sent.Count,
			notifications.Store.Entries.Count,
			scenarios,
			checks
		);
	}

	private static async Task<Result<AuthenticationResult>> CreateAndLoginManagerAsync(
		MemoryPrincipalServices identity,
		CancellationToken token
	)
	{
		var manager = new Principal(
			PrincipalId.New(),
			"Marta Manager",
			"marta@example.test",
			Permissions: new HashSet<PrincipalPermission> { ManagerPermission }
		);

		identity.Principals.Add(manager, "marta");
		var login = await identity.Authentication.AuthenticateAsync(
			new AuthenticationRequest(MemoryAuthenticationMethods.Principal, Name: "marta"),
			token
		);

		if (login.Value?.Session is not null)
			identity.SessionSource.SetSession(login.Value.Session.SessionId);

		return login;
	}

	private static async Task<StorageDeskScenarioResult> RunMemoryScenarioAsync(
		MemoryPrincipalServices identity,
		MemoryNotificationServices notifications,
		IClock clock,
		CancellationToken token
	)
	{
		// Memory provider keeps repository and journal data in memory.
		var provider = new MemoryStorageProvider();
		return await RunScenarioAsync(
			"memory",
			provider.CreateRepository<string, Ticket>("tickets"),
			provider.CreateJournal<TicketActivity>("ticket-activity"),
			identity,
			notifications,
			clock,
			token
		);
	}

	private static async Task<StorageDeskScenarioResult> RunFileScenarioAsync(
		MemoryPrincipalServices identity,
		MemoryNotificationServices notifications,
		IClock clock,
		CancellationToken token
	)
	{
		// File provider shows the same repository/journal contracts backed by JSON files.
		var rootDirectory = Path.Combine(Path.GetTempPath(), $"leva-storagedesk-files-{Guid.NewGuid():N}");
		var provider = new FileStorageProvider(rootDirectory);
		return await RunScenarioAsync(
			"files",
			provider.CreateRepository<string, Ticket>("tickets"),
			provider.CreateJournal<TicketActivity>("ticket-activity"),
			identity,
			notifications,
			clock,
			token
		);
	}

	private static async Task<StorageDeskScenarioResult> RunSqliteScenarioAsync(
		MemoryPrincipalServices identity,
		MemoryNotificationServices notifications,
		IClock clock,
		CancellationToken token
	)
	{
		// SQLite provider shows the same contracts backed by one database file.
		var databasePath = Path.Combine(Path.GetTempPath(), $"leva-storagedesk-{Guid.NewGuid():N}.sqlite");
		var provider = new SqliteStorageProvider(databasePath);
		return await RunScenarioAsync(
			"sqlite",
			provider.CreateRepository<string, Ticket>("tickets"),
			provider.CreateJournal<TicketActivity>("ticket-activity"),
			identity,
			notifications,
			clock,
			token
		);
	}

	private static async Task<StorageDeskScenarioResult> RunScenarioAsync(
		string providerName,
		IRepository<string, Ticket> tickets,
		IJournal<TicketActivity> activities,
		MemoryPrincipalServices identity,
		MemoryNotificationServices notifications,
		IClock clock,
		CancellationToken token
	)
	{
		// The application object receives only contracts and services; it does not know which provider was chosen.
		var app = new StorageDeskApp(tickets, activities, notifications.NotificationService, identity.Access, clock);
		var notificationCount = notifications.Gateway.Sent.Count;
		var notificationEntryCount = notifications.Store.Entries.Count;
		var title = $"Sample ticket stored with {providerName}";

		var created = await app.CreateAssignAndResolveAsync(title, "Alex Agent", token);
		if (created.IsFailure)
			return StorageDeskScenarioResult.Failed(providerName, created.Error.Message);

		var loaded = await tickets.LoadAsync(created.Value!.Id, token);
		if (loaded.IsFailure)
			return StorageDeskScenarioResult.Failed(providerName, loaded.Error.Message);

		var activityEntries = await activities.ReadAsync(token: token);
		if (activityEntries.IsFailure)
			return StorageDeskScenarioResult.Failed(providerName, activityEntries.Error.Message);

		var ticket = loaded.Value!.Value;
		var activityCount = activityEntries.Value!.Count;
		var sentDelta = notifications.Gateway.Sent.Count - notificationCount;
		var storedDelta = notifications.Store.Entries.Count - notificationEntryCount;
		var passed =
			ticket.Title == title
			&& ticket.AssignedTo == "Alex Agent"
			&& ticket.Status == TicketStatus.Resolved
			&& activityCount == 2
			&& sentDelta == 2
			&& storedDelta == 2;

		return new StorageDeskScenarioResult(
			providerName,
			passed,
			passed ? "OK" : "Scenario result did not match expected values.",
			ticket.Title,
			ticket.AssignedTo ?? "-",
			ticket.Status.ToString(),
			activityCount,
			sentDelta,
			storedDelta
		);
	}

	private static StorageDeskCheck Check(string name, bool passed) => new(name, passed);
}
