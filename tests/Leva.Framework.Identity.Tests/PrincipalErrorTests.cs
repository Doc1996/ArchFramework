using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class PrincipalErrorTests
{
	[Fact]
	public void NotFound_CreatesStructuredError()
	{
		var error = PrincipalErrors.NotFound("principal", "user-1");

		Assert.Equal("principal.not_found", error.Code);
		Assert.Contains("principal", error.Message);
		Assert.Contains("user-1", error.Message);
	}

	[Fact]
	public void Unavailable_AppendsDetailsWhenProvided()
	{
		var error = PrincipalErrors.Unavailable("local", "database offline");

		Assert.Equal("principal.unavailable", error.Code);
		Assert.Contains("local", error.Message);
		Assert.Contains("database offline", error.Message);
	}

	[Fact]
	public void Failed_AppendsDetailsWhenProvided()
	{
		var error = PrincipalErrors.Failed("authenticate", "bad response");

		Assert.Equal("principal.failed", error.Code);
		Assert.Contains("authenticate", error.Message);
		Assert.Contains("bad response", error.Message);
	}

	[Fact]
	public void Unauthorized_CreatesStructuredError()
	{
		var error = PrincipalErrors.Unauthorized("missing session");
		Assert.Equal("principal.unauthorized", error.Code);
		Assert.Contains("missing session", error.Message);
	}
}
