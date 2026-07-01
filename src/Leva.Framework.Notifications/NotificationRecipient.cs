namespace Leva.Framework.Notifications;

/// <summary>
/// Identifies who should receive a notification.
/// </summary>
public sealed record NotificationRecipient(string Id, string? Address = null, string? DisplayName = null);
