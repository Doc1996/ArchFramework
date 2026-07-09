namespace Leva.Framework.Presentation;

/// <summary>
/// Shows user-facing dialogs through a concrete presentation technology.
/// </summary>
public interface IDialogService
{
	Task<DialogResult> ShowAsync(DialogRequest request, CancellationToken token = default);
}
