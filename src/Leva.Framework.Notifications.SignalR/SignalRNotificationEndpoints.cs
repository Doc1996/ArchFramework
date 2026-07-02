using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// Maps the SignalR notification hub into an ASP.NET Core endpoint route builder.
/// </summary>
public static class SignalRNotificationEndpoints
{
	private const string DefaultHubPath = "/notifications";

	public static IEndpointConventionBuilder MapSignalRNotifications(
		this IEndpointRouteBuilder endpoints,
		string pattern = DefaultHubPath
	)
	{
		ArgumentNullException.ThrowIfNull(endpoints);
		ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

		return endpoints.MapHub<SignalRNotificationHub>(pattern);
	}
}
