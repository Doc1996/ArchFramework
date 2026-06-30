using System.Net.Http.Headers;
using System.Text.Json;
using Leva.Framework.Core;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Authenticates Google access tokens and maps Google user information to framework principals.
/// </summary>
public sealed class AspNetGoogleAuthenticationPolicy(
	IHttpClientFactory httpClientFactory,
	IOptions<AspNetGoogleOptions> options,
	AspNetGooglePrincipalMapper principalMapper
) : AuthenticationPolicy
{
	private readonly AspNetGoogleOptions _options = options.Value;
	public override AuthenticationMethod Method => _options.Method;

	public override async Task<Result<AuthenticationResult>> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(request);

		if (string.IsNullOrWhiteSpace(request.Token))
			return Result<AuthenticationResult>.Ok(AuthenticationResult.Failed("Google access token is missing."));

		try
		{
			var user = await LoadUserInfoAsync(request.Token, token);
			var principal = principalMapper.Map(user);
			return Result<AuthenticationResult>.Ok(AuthenticationResult.Succeeded(principal));
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<AuthenticationResult>.Fail(
				PrincipalErrors.Failed("authenticate Google principal", ex.Message)
			);
		}
	}

	private async Task<AspNetGoogleUserInfo> LoadUserInfoAsync(string accessToken, CancellationToken token)
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoEndpoint);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

		var httpClient = httpClientFactory.CreateClient(AspNetGoogleDefaults.HttpClientName);
		using var response = await httpClient.SendAsync(request, token);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(token);
		using var json = await JsonDocument.ParseAsync(stream, cancellationToken: token);
		var root = json.RootElement;

		var subject = GetRequiredString(root, "sub");
		var name = GetString(root, "name") ?? GetString(root, "email") ?? subject;
		var email = GetString(root, "email");
		var verified = GetBool(root, "email_verified");
		var picture = GetString(root, "picture");

		return new AspNetGoogleUserInfo(subject, name, email, verified, picture);
	}

	private static string GetRequiredString(JsonElement element, string name) =>
		GetString(element, name) ?? throw new InvalidOperationException($"Google user info is missing '{name}'.");

	private static string? GetString(JsonElement element, string name) =>
		element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
			? property.GetString()
			: null;

	private static bool GetBool(JsonElement element, string name) =>
		element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.True;
}
