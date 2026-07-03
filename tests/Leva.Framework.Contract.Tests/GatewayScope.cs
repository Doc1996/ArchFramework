using Leva.Framework.Notifications;

namespace Leva.Framework.Contract.Tests;

public sealed record GatewayScope(INotificationGateway Gateway, Func<IReadOnlyList<Notification>> Sent);
