using Leva.Framework.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// Registers SignalR notification gateway services.
/// </summary>
public static class SignalRNotificationServices
{
	public static IServiceCollection AddSignalRNotifications(
		this IServiceCollection services,
		Action<SignalRNotificationOptions>? configure = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);
		var options = new SignalRNotificationOptions();

		configure?.Invoke(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(options.Channel.Value);

		services.AddLogging();
		services.AddSignalR();
		services.AddSingleton(Options.Create(options));
		services.AddSingleton<SignalRNotificationGateway>();
		services.AddSingleton<INotificationGateway>(provider =>
			provider.GetRequiredService<SignalRNotificationGateway>()
		);

		return services;
	}
}
