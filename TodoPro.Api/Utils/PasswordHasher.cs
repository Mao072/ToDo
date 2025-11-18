using System.Security.Cryptography;

namespace TodoPro.Api.Utils;

public static class PasswordHasher
{
    // PBKDF2 - store salt + hash
    public static string Hash(string password, int iter = 100_000)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var rfc = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
        var hash = rfc.GetBytes(32);
        var result = new byte[1 + 4 + salt.Length + hash.Length];
        // format: version(1) | iterations(4-be) | salt | hash
        result[0] = 1;
        BitConverter.GetBytes(iter).CopyTo(result, 1);
        Array.Reverse(result, 1, 4); // convert to big-endian
        salt.CopyTo(result, 5);
        hash.CopyTo(result, 5 + salt.Length);
        return Convert.ToBase64String(result);
    }

    public static bool Verify(string encoded, string password)
    {
        try
        {
            var data = Convert.FromBase64String(encoded);
            if (data[0] != 1) return false; // unknown version
            var iterBytes = new byte[4];
            Array.Copy(data, 1, iterBytes, 0, 4);
            Array.Reverse(iterBytes);
            var iterations = BitConverter.ToInt32(iterBytes, 0);
            var salt = new byte[16];
            Array.Copy(data, 5, salt, 0, 16);
            var hash = new byte[32];
            Array.Copy(data, 5 + 16, hash, 0, 32);
            var rfc = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var newHash = rfc.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(hash, newHash);
        }
        catch
        {
            return false;
        }
    }
}
