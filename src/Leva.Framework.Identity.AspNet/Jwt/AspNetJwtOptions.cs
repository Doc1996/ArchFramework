namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Configures built in ASP.NET Core JWT issuing and validation.
/// </summary>
public sealed class AspNetJwtOptions
{
	public AuthenticationMethod Method { get; set; } = AspNetJwtDefaults.Method;
	public string AuthenticationScheme { get; set; } = AspNetJwtDefaults.AuthenticationScheme;
	public string Issuer { get; set; } = "Framework";
	public string Audience { get; set; } = "Framework";
	public string SigningKey { get; set; } = string.Empty;
	public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(1);
	public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(1);
}
