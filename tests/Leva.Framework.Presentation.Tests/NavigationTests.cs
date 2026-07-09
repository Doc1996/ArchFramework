using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class NavigationTests
{
	[Fact]
	public void Navigation_CanBeImplementedByPresentationHost()
	{
		var navigation = new TestNavigation();
		navigation.To("/plans/1", replace: true);

		Assert.Equal("/plans/1", navigation.Path);
		Assert.True(navigation.Replace);
	}

	private sealed class TestNavigation : INavigation
	{
		public string? Path { get; private set; }
		public bool Replace { get; private set; }

		public void To(string path, bool replace = false)
		{
			Path = path;
			Replace = replace;
		}
	}
}
