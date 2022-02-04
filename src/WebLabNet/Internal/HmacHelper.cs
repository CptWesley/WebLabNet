using System;
using System.Security.Cryptography;
using System.Text;

namespace WebLabNet.Internal
{
    /// <summary>
    /// Provides helper methods to deal with HMAC SHA-256 hashing.
    /// </summary>
    internal static class HmacHelper
    {
        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <param name="key">The key to hash it with.</param>
        /// <returns>The found hash.</returns>
        public static string Compute(string input, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using HMACSHA256 hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
