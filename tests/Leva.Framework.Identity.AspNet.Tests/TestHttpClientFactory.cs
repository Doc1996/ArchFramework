namespace Leva.Framework.Identity.AspNet.Tests;

internal sealed class TestHttpClientFactory : IHttpClientFactory
{
	public HttpClient CreateClient(string name) => new();
}
