using Microsoft.AspNetCore.SignalR;

namespace Leva.Framework.Sample.LiveDashboard;

internal sealed class QueryStringUserIdProvider : IUserIdProvider
{
	// The sample maps /notifications?user=demo to Clients.User("demo").
	public string? GetUserId(HubConnectionContext connection) =>
		connection.GetHttpContext()?.Request.Query["user"].FirstOrDefault();
}
