namespace Leva.Framework.Identity.Local;

/// <summary>
/// Creates and verifies protected local credential secrets.
/// </summary>
public interface ILocalSecretProtector
{
	LocalCredentialSecret Protect(string secret);
	bool Verify(string secret, LocalCredentialSecret protectedSecret);
}
