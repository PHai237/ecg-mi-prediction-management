using System.Security.Cryptography;

namespace ECG.Api.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;      // 128-bit
        private const int KeySize = 32;       // 256-bit
        private const int Iterations = 100_000;

        // format: v1|100000|saltB64|hashB64
        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );

            var key = pbkdf2.GetBytes(KeySize);

            return $"v1|{Iterations}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string stored)
        {
            var parts = stored.Split('|');
            if (parts.Length != 4) return false;
            if (parts[0] != "v1") return false;

            if (!int.TryParse(parts[1], out var iterations)) return false;

            var salt = Convert.FromBase64String(parts[2]);
            var storedKey = Convert.FromBase64String(parts[3]);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256
            );

            var key = pbkdf2.GetBytes(storedKey.Length);

            return CryptographicOperations.FixedTimeEquals(key, storedKey);
        }
    }
}
