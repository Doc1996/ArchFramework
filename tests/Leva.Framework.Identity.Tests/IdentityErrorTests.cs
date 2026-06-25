using Xunit;

namespace Leva.Framework.Identity.Tests;

public sealed class IdentityErrorTests
{
	[Fact]
	public void NotFound_CreatesStructuredError()
	{
		var error = IdentityErrors.NotFound("identity", "user-1");

		Assert.Equal("identity.not_found", error.Code);
		Assert.Contains("identity", error.Message);
		Assert.Contains("user-1", error.Message);
	}

	[Fact]
	public void Unavailable_AppendsDetailsWhenProvided()
	{
		var error = IdentityErrors.Unavailable("local", "database offline");

		Assert.Equal("identity.unavailable", error.Code);
		Assert.Contains("local", error.Message);
		Assert.Contains("database offline", error.Message);
	}

	[Fact]
	public void Failed_AppendsDetailsWhenProvided()
	{
		var error = IdentityErrors.Failed("authenticate", "bad response");

		Assert.Equal("identity.failed", error.Code);
		Assert.Contains("authenticate", error.Message);
		Assert.Contains("bad response", error.Message);
	}

	[Fact]
	public void Unauthorized_CreatesStructuredError()
	{
		var error = IdentityErrors.Unauthorized("missing session");
		Assert.Equal("identity.unauthorized", error.Code);
		Assert.Equal("missing session", error.Message);
	}
}
