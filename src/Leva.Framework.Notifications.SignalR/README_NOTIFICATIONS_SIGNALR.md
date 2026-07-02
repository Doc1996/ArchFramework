# Leva.Framework.Notifications.SignalR

`Leva.Framework.Notifications.SignalR` is the SignalR notification gateway for `Leva.Framework.Notifications`. It sends framework notifications to connected ASP.NET Core SignalR users through a strongly typed hub and client payload contract.

## Purpose and dependencies

Notifications.SignalR exists so web hosts can deliver live in-app/browser notifications without putting ASP.NET Core or SignalR concepts into the provider-neutral Notifications library. It does not implement email, mobile push, browser push subscriptions, notification storage, queues, retries, templates, user preferences, read/unread inbox state, or application-specific notification categories.

`Leva.Framework.Notifications.SignalR` depends on `Leva.Framework.Notifications`, `Leva.Framework.Core`, and the ASP.NET Core shared framework. Notifications remains provider-neutral and must not depend on SignalR. Applications are responsible for registering an `INotificationStore`, registering `NotificationService`, configuring ASP.NET Core authentication/user identifiers when user targeting is required, and mapping the notification hub endpoint.

```text
Leva.Framework.Notifications.SignalR
  -> Leva.Framework.Notifications
  -> Leva.Framework.Core
  -> Microsoft.AspNetCore.App

Leva.Framework.Notifications
  -> Leva.Framework.Core
  -> .NET base libraries
```

## Project overview

The SignalR provider is a delivery gateway. `SignalRNotificationGateway` implements `INotificationGateway`, validates that the notification channel matches the configured SignalR channel, maps the framework `Notification` to a `SignalRNotificationPayload`, and sends it to `Clients.User(notification.Recipient.Id)`. The recipient id is therefore the SignalR user id. ASP.NET Core authentication or a custom SignalR `IUserIdProvider` should decide how authenticated users map to those ids.

The provider uses a strongly typed SignalR hub. `SignalRNotificationHub` exposes `ISignalRNotificationClient`, and clients receive payloads through `ReceiveNotification`. If no browser/client is connected for the targeted user, SignalR still treats the send as successful; offline delivery, durable inboxes, and retry behavior belong to a store, queue, or future provider layer, not to this gateway.

`SignalRNotificationServices` registers SignalR, gateway options, the concrete `SignalRNotificationGateway`, and the `INotificationGateway` mapping. It does not register `NotificationService` or any `INotificationStore` because those are application composition decisions. `SignalRNotificationEndpoints` maps the hub at `/notifications` or a caller-provided path.

## Files and classes

### Options

`SignalRNotificationOptions` - Configures the notification channel accepted by the SignalR gateway and owns the default `signalr` channel value.

### Gateway and hub

`SignalRNotificationGateway` - Implements `INotificationGateway`, validates channel and recipient id, sends payloads to SignalR users, and returns structured notification results.
`SignalRNotificationHub` - Strongly typed SignalR hub used by connected clients to receive notifications.
`ISignalRNotificationClient` - SignalR client contract with `ReceiveNotification`.
`SignalRNotificationPayload` - Stable payload sent to browser/client code.

### ASP.NET Core registration

`SignalRNotificationServices` - Registers SignalR notification services in ASP.NET Core dependency injection.
`SignalRNotificationEndpoints` - Maps the SignalR notification hub endpoint.
