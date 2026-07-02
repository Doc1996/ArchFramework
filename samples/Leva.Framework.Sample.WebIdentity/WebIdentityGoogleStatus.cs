namespace Leva.Framework.Sample.WebIdentity;

internal sealed record WebIdentityGoogleStatus(bool IsConfigured, string LoginUrl, string CallbackUrl, string Message);
