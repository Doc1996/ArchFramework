# Leva.Framework.Notifications.SignalR.Tests

`Leva.Framework.Notifications.SignalR.Tests` verifies the SignalR notification gateway and ASP.NET Core registration helpers from `Leva.Framework.Notifications.SignalR`. These tests cover payload mapping, user targeting, unsupported channel failures, missing recipient failures, client send failures, and service registration.

## Coverage

`SignalRNotificationGatewayTests` - Verifies that SignalR notifications are mapped to payloads and sent to `Clients.User(recipient.Id)`, unsupported channels are rejected, missing recipient ids fail, and client send exceptions return failed results.
`SignalRNotificationServiceTests` - Verifies SignalR notification service registration and custom channel registration.
