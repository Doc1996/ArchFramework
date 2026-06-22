namespace Leva.Framework.Core;

/// <summary>
/// Defines dispatch priority for events waiting in the engine queue.
/// </summary>
public enum EventPriority
{
	Low = 0,
	Normal = 1,
	High = 2,
	Critical = 3,
}
