namespace Leva.Framework.Presentation;

/// <summary>
/// Provides a presentation-level navigation capability without binding application code to a specific UI host.
/// </summary>
public interface INavigation
{
	void To(string path, bool replace = false);
}
