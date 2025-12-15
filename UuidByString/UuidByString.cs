using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UuidByString
{
    public static class UuidByString
    {
        private static readonly char[] HexDigits = "0123456789abcdef".ToCharArray();
        private static readonly byte[] EmptyByteArray = [];

        /// <summary>
        /// Generates UUID with default version 5 (SHA-1)
        /// </summary>
        /// <param name="target">The string to generate UUID from</param>
        /// <returns>Generated UUID string</returns>
        public static string GenerateUuid(string target)
        {
            return GenerateUuid(target, null, 5);
        }

        /// <summary>
        /// Generates UUID with specified version
        /// </summary>
        /// <param name="target">The string to generate UUID from</param>
        /// <param name="version">Version of UUID (3 for MD5, 5 for SHA-1)</param>
        /// <returns>Generated UUID string</returns>
        public static string GenerateUuid(string target, int version)
        {
            return GenerateUuid(target, null, version);
        }

        /// <summary>
        /// Generates UUID with namespace and default version 5 (SHA-1)
        /// </summary>
        /// <param name="target">The string to generate UUID from</param>
        /// <param name="namespace">UUID namespace</param>
        /// <returns>Generated UUID string</returns>
        public static string GenerateUuid(string target, string @namespace)
        {
            return GenerateUuid(target, @namespace, 5);
        }

        /// <summary>
        /// Generates UUID with namespace and specified version
        /// </summary>
        /// <param name="target">The string to generate UUID from</param>
        /// <param name="namespace">UUID namespace</param>
        /// <param name="version">Version of UUID (3 for MD5, 5 for SHA-1)</param>
        /// <returns>Generated UUID string</returns>
        public static string GenerateUuid(string target, string @namespace, int version)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (version != 3 && version != 5)
            {
                throw new ArgumentException("Version of UUID can be only 3 or 5", nameof(version));
            }

            var targetCharBuffer = Encoding.UTF8.GetBytes(target);
            var namespaceCharBuffer = @namespace != null ? ParseUuid(@namespace) : EmptyByteArray;
            var buffer = ConcatBuffers(namespaceCharBuffer, targetCharBuffer);

            byte[] hash;
            using (var algorithm = version == 3 ? (HashAlgorithm)MD5.Create() : SHA1.Create())
            {
                hash = algorithm.ComputeHash(buffer);
            }

            return HashToUuid(hash, version);
        }

        private static byte[] ConcatBuffers(byte[] buf1, byte[] buf2)
        {
            var result = new byte[buf1.Length + buf2.Length];
            Buffer.BlockCopy(buf1, 0, result, 0, buf1.Length);
            Buffer.BlockCopy(buf2, 0, result, buf1.Length, buf2.Length);
            return result;
        }

        private static byte[] ParseUuid(string uuid)
        {
            if (!ValidateUuid(uuid))
            {
                throw new ArgumentException("Invalid UUID", nameof(uuid));
            }

            var buf = new byte[16];
            var strIndex = 0;
            var bufIndex = 0;

            while (strIndex < uuid.Length)
            {
                if (uuid[strIndex] == '-')
                {
                    strIndex++;
                    continue;
                }

                var oct = (uuid[strIndex].ToString() + uuid[strIndex + 1]).ToLower();
                buf[bufIndex] = Convert.ToByte(oct, 16);

                bufIndex++;
                strIndex += 2;
            }

            return buf;
        }

        private static bool ValidateUuid(string uuid)
        {
            return uuid.Length == 36 && UuidRegex.IsMatch(uuid);
        }

        private static readonly Regex UuidRegex = new Regex(
            @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static string HashToUuid(IList<byte> hashBuffer, int version)
        {
            var hexBuilder = new StringBuilder(36);

            hexBuilder.Append(Uint8ArrayToHex([.. hashBuffer.Take(4)])).Append('-');
            hexBuilder.Append(Uint8ArrayToHex([.. hashBuffer.Skip(4).Take(2)])).Append('-');
            hexBuilder.Append(Uint8ToHex((byte)(hashBuffer[6] & 0x0f | (byte)(version * 0x10))))
                .Append(Uint8ToHex(hashBuffer[7])).Append('-');
            hexBuilder.Append(Uint8ToHex((byte)(hashBuffer[8] & 0x3f | 0x80))).Append(Uint8ToHex(hashBuffer[9]))
                .Append('-');
            hexBuilder.Append(Uint8ArrayToHex([.. hashBuffer.Skip(10).Take(6)]));

            return hexBuilder.ToString();
        }

        private static string Uint8ToHex(byte uByte)
        {
            var first = uByte >> 4;
            var second = uByte - (byte)(first << 4);

            return $"{HexDigits[first]}{HexDigits[second]}";
        }

        private static string Uint8ArrayToHex(byte[] buf)
        {
            var hexBuilder = new StringBuilder(buf.Length * 2);
            foreach (var b in buf)
            {
                hexBuilder.Append(Uint8ToHex(b));
            }

            return hexBuilder.ToString();
        }
    }
}