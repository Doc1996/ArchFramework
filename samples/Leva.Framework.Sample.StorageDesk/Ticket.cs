namespace Leva.Framework.Sample.StorageDesk;

internal sealed record Ticket(
	string Id,
	string Title,
	string CreatedBy,
	string? AssignedTo,
	TicketStatus Status,
	DateTimeOffset CreatedAt,
	DateTimeOffset? ResolvedAt = null
)
{
	public Ticket AssignTo(string assignee) => this with { AssignedTo = assignee, Status = TicketStatus.Assigned };

	public Ticket Resolve(DateTimeOffset resolvedAt) =>
		this with
		{
			Status = TicketStatus.Resolved,
			ResolvedAt = resolvedAt,
		};
}
