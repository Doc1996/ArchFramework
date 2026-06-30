using System.Security.Cryptography;
using System.Text.Json;
using Leva.Framework.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Handles the built in ASP.NET Core Google OAuth sign-in flow.
/// </summary>
public sealed class AspNetGoogleSignInService(
	IOptions<AspNetGoogleOptions> options,
	IHttpClientFactory httpClientFactory,
	AspNetIdentityService identity
)
{
	private readonly AspNetGoogleOptions _options = options.Value;

	public string CreateAuthorizationUrl(HttpContext context, string? returnUrl = null)
	{
		ArgumentNullException.ThrowIfNull(context);
		ValidateOptions();
		var state = CreateState();

		WriteCorrelationCookies(context, state, NormalizeReturnUrl(returnUrl));
		var query = new Dictionary<string, string?>
		{
			["client_id"] = _options.ClientId,
			["redirect_uri"] = GetRedirectUri(context),
			["response_type"] = "code",
			["scope"] = _options.Scope,
			["state"] = state,
		};

		return QueryHelpers.AddQueryString(_options.AuthorizationEndpoint, query);
	}

	public async Task<Result<string>> CompleteLoginAsync(HttpContext context, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(context);

		ValidateOptions();
		var code = context.Request.Query["code"].FirstOrDefault();
		var state = context.Request.Query["state"].FirstOrDefault();

		if (string.IsNullOrWhiteSpace(code))
			return Result<string>.Fail(PrincipalErrors.Invalid("Google callback", "Authorization code is missing."));

		if (!IsValidState(context, state))
			return Result<string>.Fail(PrincipalErrors.Invalid("Google callback", "OAuth state is invalid."));

		var accessToken = await ExchangeCodeAsync(context, code, token);
		var request = new AuthenticationRequest(_options.Method, Token: accessToken);
		var identityResult = await identity.LoginAsync(context, request, token);

		if (identityResult.IsFailure)
			return Result<string>.Fail(identityResult.Error);

		if (!identityResult.Value!.IsAuthenticated)
			return Result<string>.Fail(PrincipalErrors.Unauthorized(identityResult.Value.Reason));

		var returnUrl = NormalizeReturnUrl(context.Request.Cookies[_options.ReturnUrlCookieName]);
		DeleteCorrelationCookies(context);
		return Result<string>.Ok(returnUrl);
	}

	private async Task<string> ExchangeCodeAsync(HttpContext context, string code, CancellationToken token)
	{
		var values = new Dictionary<string, string>
		{
			["client_id"] = _options.ClientId,
			["client_secret"] = _options.ClientSecret,
			["code"] = code,
			["grant_type"] = "authorization_code",
			["redirect_uri"] = GetRedirectUri(context),
		};

		var httpClient = httpClientFactory.CreateClient(AspNetGoogleDefaults.HttpClientName);
		using var response = await httpClient.PostAsync(
			_options.TokenEndpoint,
			new FormUrlEncodedContent(values),
			token
		);

		response.EnsureSuccessStatusCode();
		await using var stream = await response.Content.ReadAsStreamAsync(token);
		using var json = await JsonDocument.ParseAsync(stream, cancellationToken: token);

		if (
			!json.RootElement.TryGetProperty("access_token", out var accessToken)
			|| accessToken.ValueKind != JsonValueKind.String
			|| string.IsNullOrWhiteSpace(accessToken.GetString())
		)
			throw new InvalidOperationException("Google token response is missing access token.");

		return accessToken.GetString()!;
	}

	private string GetRedirectUri(HttpContext context) =>
		$"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{_options.CallbackPath}";

	private void WriteCorrelationCookies(HttpContext context, string state, string returnUrl)
	{
		var cookieOptions = CreateCookieOptions();
		context.Response.Cookies.Append(_options.StateCookieName, state, cookieOptions);
		context.Response.Cookies.Append(_options.ReturnUrlCookieName, returnUrl, cookieOptions);
	}

	private void DeleteCorrelationCookies(HttpContext context)
	{
		var cookieOptions = CreateCookieOptions();
		context.Response.Cookies.Delete(_options.StateCookieName, cookieOptions);
		context.Response.Cookies.Delete(_options.ReturnUrlCookieName, cookieOptions);
	}

	private CookieOptions CreateCookieOptions()
	{
		return new()
		{
			HttpOnly = _options.HttpOnlyCookie,
			Secure = _options.SecureCookie,
			SameSite = _options.SameSite,
			Expires = DateTimeOffset.UtcNow.Add(_options.CorrelationLifetime),
		};
	}

	private bool IsValidState(HttpContext context, string? state)
	{
		var expected = context.Request.Cookies[_options.StateCookieName];
		return !string.IsNullOrWhiteSpace(state)
			&& !string.IsNullOrWhiteSpace(expected)
			&& string.Equals(state, expected, StringComparison.Ordinal);
	}

	private string NormalizeReturnUrl(string? returnUrl) =>
		IsLocalReturnUrl(returnUrl) ? returnUrl! : NormalizeDefaultReturnUrl();

	private string NormalizeDefaultReturnUrl() =>
		IsLocalReturnUrl(_options.DefaultReturnUrl) ? _options.DefaultReturnUrl : "/";

	private static bool IsLocalReturnUrl(string? returnUrl)
	{
		return !string.IsNullOrWhiteSpace(returnUrl)
			&& returnUrl[0] == '/'
			&& (returnUrl.Length == 1 || returnUrl[1] != '/')
			&& (returnUrl.Length == 1 || returnUrl[1] != '\\');
	}

	private static string CreateState()
	{
		Span<byte> bytes = stackalloc byte[32];
		RandomNumberGenerator.Fill(bytes);
		return WebEncoders.Base64UrlEncode(bytes);
	}

	private void ValidateOptions()
	{
		if (string.IsNullOrWhiteSpace(_options.ClientId))
			throw new InvalidOperationException("Google client id is not configured.");
		if (string.IsNullOrWhiteSpace(_options.ClientSecret))
			throw new InvalidOperationException("Google client secret is not configured.");
	}
}
