using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystemAPI.Auth
{
	public static class RefreshTokenGenerator
	{
		// 64 cryptographically random bytes, as URL-safe text
		public static string Generate() =>
			Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

		// Fast hash is correct here: the token is long and random, nothing to guess
		public static string Hash(string token) =>
			Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
	}
}