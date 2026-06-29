using System.Security.Cryptography;

namespace Leva.Framework.Identity.Local;

/// <summary>
/// Creates and verifies PBKDF2-SHA256 local credential secrets.
/// </summary>
public sealed class LocalSecretProtector : ILocalSecretProtector
{
	private const string Algorithm = "PBKDF2-SHA256";
	private readonly int _iterations;
	private readonly int _saltSize;
	private readonly int _secretSize;

	public LocalSecretProtector(int iterations = 100_000, int saltSize = 16, int secretSize = 32)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterations);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(saltSize);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(secretSize);

		_iterations = iterations;
		_saltSize = saltSize;
		_secretSize = secretSize;
	}

	public LocalCredentialSecret Protect(string secret)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(secret);

		var salt = RandomNumberGenerator.GetBytes(_saltSize);
		var value = Protect(secret, salt, _iterations, _secretSize);

		return new LocalCredentialSecret(
			Algorithm,
			_iterations,
			Convert.ToBase64String(salt),
			Convert.ToBase64String(value)
		);
	}

	public bool Verify(string secret, LocalCredentialSecret protectedSecret)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(secret);
		ArgumentNullException.ThrowIfNull(protectedSecret);

		if (protectedSecret.Algorithm != Algorithm)
			return false;

		try
		{
			var salt = Convert.FromBase64String(protectedSecret.Salt);
			var expected = Convert.FromBase64String(protectedSecret.Value);
			var actual = Protect(secret, salt, protectedSecret.Iterations, expected.Length);

			return CryptographicOperations.FixedTimeEquals(actual, expected);
		}
		catch (FormatException)
		{
			return false;
		}
	}

	private static byte[] Protect(string secret, byte[] salt, int iterations, int secretSize) =>
		Rfc2898DeriveBytes.Pbkdf2(secret, salt, iterations, HashAlgorithmName.SHA256, secretSize);
}
