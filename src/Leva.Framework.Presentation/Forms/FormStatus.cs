namespace Leva.Framework.Presentation;

/// <summary>
/// Describes the current state of one user-facing form.
/// </summary>
public enum FormStatus
{
	Clean,
	Modified,
	Submitting,
	Submitted,
	Invalid,
	Failed,
}
