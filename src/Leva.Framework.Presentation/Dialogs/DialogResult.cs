namespace Leva.Framework.Presentation;

/// <summary>
/// Represents the result of a user-facing dialog interaction.
/// </summary>
public sealed record DialogResult(bool Accepted)
{
	public static DialogResult Accept() => new(true);

	public static DialogResult Cancel() => new(false);
}
