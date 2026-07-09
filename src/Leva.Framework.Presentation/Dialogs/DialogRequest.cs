namespace Leva.Framework.Presentation;

/// <summary>
/// Describes one user-facing dialog request.
/// </summary>
public sealed record DialogRequest(
	string Message,
	string? Title = null,
	string AcceptText = "OK",
	string? CancelText = null
)
{
	public bool CanCancel => !string.IsNullOrWhiteSpace(CancelText);
}
