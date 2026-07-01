# Leva.Framework.Notifications.Tests

`Leva.Framework.Notifications.Tests` verifies the provider-neutral notification values and service behavior from `Leva.Framework.Notifications`. These tests cover channel values, notification creation, gateway/store coordination, sent and failed entries, and store failure propagation.

## Coverage

`NotificationValueTests` - Verifies provider-defined notification channels and rejection of empty channel values.
`NotificationServiceTests` - Verifies notification creation, gateway/store coordination, sent and failed entry recording, and store failure propagation.
