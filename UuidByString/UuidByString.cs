using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UuidByString
{
    /// <summary>
    /// Provides methods for generating deterministic RFC-4122 Name-Based UUIDs from strings.
    /// Supports UUID version 3 (MD5) and version 5 (SHA-1).
    /// </summary>
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

#if NET9
            return ParseUuidSpan(uuid.AsSpan());
#else
            return ParseUuidLegacy(uuid);
#endif
        }

#if NET9
        private static byte[] ParseUuidSpan(ReadOnlySpan<char> uuid)
        {
            var buf = new byte[16];
            var bufIndex = 0;

            for (var i = 0; i < uuid.Length && bufIndex < 16; i++)
            {
                if (uuid[i] == '-')
                {
                    continue;
                }

                // Parse two hex digits directly
                var high = HexCharToValue(uuid[i]);
                var low = HexCharToValue(uuid[i + 1]);
                buf[bufIndex++] = (byte)((high << 4) | low);
                i++; // Skip the second hex digit
            }

            return buf;
        }
#else
        private static byte[] ParseUuidLegacy(string uuid)
        {
            var buf = new byte[16];
            var strIndex = 0;
            var bufIndex = 0;

            while (strIndex < uuid.Length && bufIndex < 16)
            {
                if (uuid[strIndex] == '-')
                {
                    strIndex++;
                    continue;
                }

                // Parse two hex characters directly without string allocation
                var high = HexCharToValue(uuid[strIndex]);
                var low = HexCharToValue(uuid[strIndex + 1]);
                buf[bufIndex++] = (byte)((high << 4) | low);
                strIndex += 2;
            }

            return buf;
        }
#endif

        private static int HexCharToValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return 0;
        }

        private static bool ValidateUuid(string uuid)
        {
            return uuid.Length == 36 && UuidRegex.IsMatch(uuid);
        }

        private static readonly Regex UuidRegex = new Regex(
            @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static string HashToUuid(byte[] hashBuffer, int version)
        {
            var hexBuilder = new StringBuilder(36);

            // Direct byte indexing instead of LINQ operations
            // First group: 8 hex chars (4 bytes)
            AppendHexBytes(hexBuilder, hashBuffer, 0, 4);
            hexBuilder.Append('-');

            // Second group: 4 hex chars (2 bytes)
            AppendHexBytes(hexBuilder, hashBuffer, 4, 2);
            hexBuilder.Append('-');

            // Third group: 4 hex chars (2 bytes) with version modification
            AppendHexByte(hexBuilder, (byte)((hashBuffer[6] & 0x0f) | (version << 4)));
            AppendHexByte(hexBuilder, hashBuffer[7]);
            hexBuilder.Append('-');

            // Fourth group: 4 hex chars (2 bytes) with variant modification
            AppendHexByte(hexBuilder, (byte)((hashBuffer[8] & 0x3f) | 0x80));
            AppendHexByte(hexBuilder, hashBuffer[9]);
            hexBuilder.Append('-');

            // Fifth group: 12 hex chars (6 bytes)
            AppendHexBytes(hexBuilder, hashBuffer, 10, 6);

            return hexBuilder.ToString();
        }

        private static void AppendHexBytes(StringBuilder builder, byte[] bytes, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                AppendHexByte(builder, bytes[offset + i]);
            }
        }

        private static void AppendHexByte(StringBuilder builder, byte b)
        {
            builder.Append(HexDigits[b >> 4]);
            builder.Append(HexDigits[b & 0x0F]);
        }
    }
}