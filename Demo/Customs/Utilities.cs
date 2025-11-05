using System.Security.Cryptography;

namespace Demo.Customs
{
    public class Utilities
    {
        private const int Entropy = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public string hashPass(string text)
        {
            byte[] entropy = new byte[Entropy];
            RandomNumberGenerator.Fill(entropy);

            var pbkdf2 = new Rfc2898DeriveBytes(text, entropy, Iterations, HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(HashSize);

            byte[] hashBytes = new byte[Entropy + HashSize];
            Array.Copy(entropy, 0, hashBytes, 0, Entropy);
            Array.Copy(hash, 0, hashBytes, Entropy, HashSize);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
