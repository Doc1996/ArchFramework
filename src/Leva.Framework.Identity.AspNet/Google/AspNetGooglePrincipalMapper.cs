namespace Leva.Framework.Identity.AspNet;

/// <summary>
/// Maps Google user information to framework principals.
/// </summary>
public sealed class AspNetGooglePrincipalMapper
{
	public Principal Map(AspNetGoogleUserInfo user)
	{
		ArgumentNullException.ThrowIfNull(user);

		var claims = new List<PrincipalClaim>
		{
			new(AspNetGoogleDefaults.SubjectClaimType, user.Subject),
			new("email_verified", user.EmailVerified.ToString()),
		};

		if (!string.IsNullOrWhiteSpace(user.Picture))
			claims.Add(new PrincipalClaim(AspNetGoogleDefaults.PictureClaimType, user.Picture));

		return new Principal(new PrincipalId($"google:{user.Subject}"), user.DisplayName, user.Email, Claims: claims);
	}
}
