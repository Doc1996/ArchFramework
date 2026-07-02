using Leva.Framework.Core;
using Leva.Framework.Identity;
using Leva.Framework.Notifications;
using Leva.Framework.Storage;

namespace Leva.Framework.Sample.StorageDesk;

internal sealed class StorageDeskApp(
	IRepository<string, Ticket> tickets,
	IJournal<TicketActivity> activities,
	NotificationService notifications,
	PrincipalAccess principalAccess,
	IClock clock
)
{
	private static readonly NotificationChannel TicketChannel = new("ticket");
	private static readonly PrincipalPermission ManageTickets = new("tickets.manage");

	public async Task<Result<Ticket>> CreateAssignAndResolveAsync(
		string title,
		string assignee,
		CancellationToken token = default
	)
	{
		// PrincipalAccess keeps authorization checks independent from ASP.NET or any specific identity provider.
		var authorization = await principalAccess.RequireAsync(ManageTickets, token);
		if (authorization.IsFailure)
			return Result<Ticket>.Fail(authorization.Error);

		if (!authorization.Value!.IsAuthorized)
			return Result<Ticket>.Fail(
				new Error("sample.ticket.unauthorized", authorization.Value.Reason ?? "Not allowed.")
			);

		var principal = await principalAccess.GetPrincipalAsync(token);
		if (principal.IsFailure)
			return Result<Ticket>.Fail(principal.Error);

		var ticket = new Ticket(
			Guid.NewGuid().ToString("N"),
			title,
			principal.Value?.DisplayName ?? "unknown",
			null,
			TicketStatus.New,
			clock.UtcNow
		).AssignTo(assignee);

		// IRepository is provider-neutral: this same code runs with memory, files, or SQLite.
		var assigned = await tickets.SaveAsync(ticket.Id, ticket, token: token);
		if (assigned.IsFailure)
			return Result<Ticket>.Fail(assigned.Error);

		// IJournal records append-only activity without coupling the app to one storage implementation.
		var assignedActivity = await activities.AppendAsync(
			new TicketActivity(ticket.Id, $"Ticket assigned to {assignee}.", clock.UtcNow),
			token
		);
		if (assignedActivity.IsFailure)
			return Result<Ticket>.Fail(assignedActivity.Error);

		var assignedNotification = await SendTicketNotificationAsync(
			assignee,
			"Ticket assigned",
			$"{ticket.Title} was assigned to you.",
			token
		);
		if (assignedNotification.IsFailure)
			return Result<Ticket>.Fail(assignedNotification.Error);

		var resolved = ticket.Resolve(clock.UtcNow);
		var savedResolved = await tickets.SaveAsync(resolved.Id, resolved, token: token);
		if (savedResolved.IsFailure)
			return Result<Ticket>.Fail(savedResolved.Error);

		var resolvedActivity = await activities.AppendAsync(
			new TicketActivity(resolved.Id, "Ticket resolved.", clock.UtcNow),
			token
		);
		if (resolvedActivity.IsFailure)
			return Result<Ticket>.Fail(resolvedActivity.Error);

		var resolvedNotification = await SendTicketNotificationAsync(
			assignee,
			"Ticket resolved",
			$"{resolved.Title} was resolved.",
			token
		);
		if (resolvedNotification.IsFailure)
			return Result<Ticket>.Fail(resolvedNotification.Error);

		return Result<Ticket>.Ok(resolved);
	}

	private async Task<Result<NotificationEntry>> SendTicketNotificationAsync(
		string assignee,
		string subject,
		string body,
		CancellationToken token
	)
	{
		// NotificationService hides the gateway/store flow behind one application-facing service.
		return await notifications.SendAsync(
			new NotificationRecipient(assignee, DisplayName: assignee),
			TicketChannel,
			subject,
			body,
			token
		);
	}
}
