using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Leva.Framework.Core;
using Microsoft.Extensions.Options;

namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Issues and validates built in ASP.NET Core JWT access tokens for framework auth sessions.
/// </summary>
public sealed class AspNetJwtTokenService(IOptions<AspNetJwtOptions> options)
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	private readonly AspNetJwtClaimsMapper _claimsMapper = new();
	private readonly AspNetJwtOptions _options = options.Value;

	public Result<AspNetJwtTokenResponse> CreateToken(AuthSession session)
	{
		ArgumentNullException.ThrowIfNull(session);
		try
		{
			ValidateOptions();
			var issuedAt = DateTimeOffset.UtcNow;
			var expiresAt = issuedAt.Add(_options.TokenLifetime);
			var payload = _claimsMapper.Map(session, _options.Issuer, _options.Audience, issuedAt, expiresAt);

			var token = WriteToken(payload);
			var response = new AspNetJwtTokenResponse(
				token,
				AspNetJwtDefaults.TokenType,
				expiresAt,
				AspNetLoginResponse.FromAuthenticationResult(new AuthenticationResult(true, session.Principal, session))
			);

			return Result<AspNetJwtTokenResponse>.Ok(response);
		}
		catch (Exception ex)
		{
			return Result<AspNetJwtTokenResponse>.Fail(PrincipalErrors.Failed("create JWT token", ex.Message));
		}
	}

	public Result<AuthSession> ValidateToken(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
			return Result<AuthSession>.Fail(PrincipalErrors.Unauthorized("JWT token is missing."));

		try
		{
			ValidateOptions();
			var parts = token.Split('.');
			if (parts.Length != 3)
				return Result<AuthSession>.Fail(PrincipalErrors.Unauthorized("JWT token is malformed."));

			var signedValue = $"{parts[0]}.{parts[1]}";
			var signature = Sign(signedValue);
			if (
				!CryptographicOperations.FixedTimeEquals(
					Encoding.ASCII.GetBytes(signature),
					Encoding.ASCII.GetBytes(parts[2])
				)
			)
				return Result<AuthSession>.Fail(PrincipalErrors.Unauthorized("JWT signature is invalid."));

			var header = ReadJson<AspNetJwtHeader>(parts[0]);
			if (!string.Equals(header.Algorithm, "HS256", StringComparison.Ordinal))
				return Result<AuthSession>.Fail(PrincipalErrors.Unauthorized("JWT algorithm is not supported."));

			var payload = ReadJson<AspNetJwtPayload>(parts[1]);
			var validation = ValidatePayload(payload);
			if (validation.IsFailure)
				return Result<AuthSession>.Fail(validation.Error);

			return Result<AuthSession>.Ok(_claimsMapper.Map(payload));
		}
		catch (Exception ex)
		{
			return Result<AuthSession>.Fail(PrincipalErrors.Unauthorized(ex.Message));
		}
	}

	private string WriteToken(AspNetJwtPayload payload)
	{
		var header = new AspNetJwtHeader("HS256", "JWT");
		var headerText = EncodeJson(header);
		var payloadText = EncodeJson(payload);
		var signedValue = $"{headerText}.{payloadText}";

		return $"{signedValue}.{Sign(signedValue)}";
	}

	private Result ValidatePayload(AspNetJwtPayload payload)
	{
		var utcNow = DateTimeOffset.UtcNow;
		if (!string.Equals(payload.Issuer, _options.Issuer, StringComparison.Ordinal))
			return Result.Fail(PrincipalErrors.Unauthorized("JWT issuer is invalid."));

		if (!string.Equals(payload.Audience, _options.Audience, StringComparison.Ordinal))
			return Result.Fail(PrincipalErrors.Unauthorized("JWT audience is invalid."));

		if (DateTimeOffset.FromUnixTimeSeconds(payload.NotBefore) > utcNow.Add(_options.ClockSkew))
			return Result.Fail(PrincipalErrors.Unauthorized("JWT token is not active yet."));

		if (DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAt) <= utcNow.Subtract(_options.ClockSkew))
			return Result.Fail(PrincipalErrors.Unauthorized("JWT token is expired."));

		return Result.Ok();
	}

	private string Sign(string value)
	{
		using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningKey));
		return Encode(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
	}

	private static string EncodeJson<T>(T value) => Encode(JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions));

	private static T ReadJson<T>(string value) =>
		JsonSerializer.Deserialize<T>(Decode(value), JsonOptions)
		?? throw new InvalidOperationException($"JWT {typeof(T).Name} could not be read.");

	private static string Encode(ReadOnlySpan<byte> value) =>
		Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

	private static byte[] Decode(string value)
	{
		var text = value.Replace('-', '+').Replace('_', '/');
		return Convert.FromBase64String(text.PadRight(text.Length + (4 - text.Length % 4) % 4, '='));
	}

	private void ValidateOptions()
	{
		if (string.IsNullOrWhiteSpace(_options.SigningKey))
			throw new InvalidOperationException("JWT signing key is not configured.");

		if (Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
			throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
	}
}
