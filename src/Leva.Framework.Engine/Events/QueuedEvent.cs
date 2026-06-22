using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Wraps an application event with the queue metadata needed for prioritised dispatch.
/// </summary>
public sealed record QueuedEvent(IEvent AppEvent, EventPriority Priority, DateTimeOffset EnqueuedAt);
