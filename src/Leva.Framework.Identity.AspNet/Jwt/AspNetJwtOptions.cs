namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Configures built in ASP.NET Core JWT issuing and validation.
/// </summary>
public sealed class AspNetJwtOptions
{
	public AuthenticationMethod Method { get; set; } = AspNetJwtDefaults.Method;
	public string AuthenticationScheme { get; set; } = AspNetJwtDefaults.AuthenticationScheme;
	public string Issuer { get; set; } = AspNetJwtDefaults.Issuer;
	public string Audience { get; set; } = AspNetJwtDefaults.Audience;
	public string SigningKey { get; set; } = string.Empty;
	public TimeSpan TokenLifetime { get; set; } = AspNetJwtDefaults.TokenLifetime;
	public TimeSpan ClockSkew { get; set; } = AspNetJwtDefaults.ClockSkew;
}
