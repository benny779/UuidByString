using System;
using System.Reflection;
using NUnit.Framework;

namespace UuidByString.Tests
{
    [TestFixture]
    public class UuidByStringTests
    {
        /// <summary>
        /// Tests the main UUID generation for "Hello world!" which should always return the same result
        /// </summary>
        [Test]
        public void GenerateUuid_HelloWorld_ReturnsExpectedResult()
        {
            var result = UuidGenerator.GenerateUuid("Hello world!");
            Assert.AreEqual("d3486ae9-136e-5856-bc42-212385ea7970", result);
        }

        /// <summary>
        /// Tests that null string throws ArgumentNullException (empty strings should be allowed)
        /// </summary>
        [Test]
        public void GenerateUuid_NullOrEmptyString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => UuidGenerator.GenerateUuid(null));
            // Empty strings should NOT throw - they're valid input as per JavaScript implementation
            Assert.DoesNotThrow(() => UuidGenerator.GenerateUuid(string.Empty));
        }

        /// <summary>
        /// Tests that invalid version throws ArgumentException
        /// </summary>
        [Test]
        public void GenerateUuid_InvalidVersion_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => UuidGenerator.GenerateUuid("Hello", version: 1));
            Assert.Throws<ArgumentException>(() => UuidGenerator.GenerateUuid("Hello", TestData.TestNamespaceUuid, 1));
            Assert.Throws<ArgumentException>(() => UuidGenerator.GenerateUuid("Hello", version: 2));
            Assert.Throws<ArgumentException>(() => UuidGenerator.GenerateUuid("Hello", version: 4));
            Assert.Throws<ArgumentException>(() => UuidGenerator.GenerateUuid("Hello", version: 6));
        }

        /// <summary>
        /// Tests UUID generation with version 3 (MD5) for various string samples
        /// </summary>
        [Test]
        public void GenerateUuid_Version3_ProducesConsistentResults()
        {
            var results = new string[TestData.StringSamples.Length];
            
            for (int i = 0; i < TestData.StringSamples.Length; i++)
            {
                results[i] = UuidGenerator.GenerateUuid(TestData.StringSamples[i], version: 3);
                // Verify the result is a valid UUID format
                Assert.IsTrue(IsValidUuidFormat(results[i]), $"Result for sample {i} is not a valid UUID format: {results[i]}");
                
                // Verify consistency - calling again should produce same result
                var secondCall = UuidGenerator.GenerateUuid(TestData.StringSamples[i], version: 3);
                Assert.AreEqual(results[i], secondCall, $"UUID generation is not consistent for sample {i}");
            }
        }

        /// <summary>
        /// Tests UUID generation with version 3 (MD5) and namespace for various string samples
        /// </summary>
        [Test]
        public void GenerateUuid_Version3WithNamespace_ProducesConsistentResults()
        {
            var results = new string[TestData.StringSamples.Length];
            
            for (int i = 0; i < TestData.StringSamples.Length; i++)
            {
                results[i] = UuidGenerator.GenerateUuid(TestData.StringSamples[i], TestData.TestNamespaceUuid, 3);
                // Verify the result is a valid UUID format
                Assert.IsTrue(IsValidUuidFormat(results[i]), $"Result for sample {i} is not a valid UUID format: {results[i]}");
                
                // Verify consistency - calling again should produce same result
                var secondCall = UuidGenerator.GenerateUuid(TestData.StringSamples[i], TestData.TestNamespaceUuid, 3);
                Assert.AreEqual(results[i], secondCall, $"UUID generation is not consistent for sample {i}");
            }
        }

        /// <summary>
        /// Tests UUID generation with version 5 (SHA-1) for various string samples
        /// </summary>
        [Test]
        public void GenerateUuid_Version5_ProducesConsistentResults()
        {
            var results = new string[TestData.StringSamples.Length];
            
            for (int i = 0; i < TestData.StringSamples.Length; i++)
            {
                results[i] = UuidGenerator.GenerateUuid(TestData.StringSamples[i], version: 5);
                // Verify the result is a valid UUID format
                Assert.IsTrue(IsValidUuidFormat(results[i]), $"Result for sample {i} is not a valid UUID format: {results[i]}");
                
                // Verify consistency - calling again should produce same result
                var secondCall = UuidGenerator.GenerateUuid(TestData.StringSamples[i], version: 5);
                Assert.AreEqual(results[i], secondCall, $"UUID generation is not consistent for sample {i}");
            }
        }

        /// <summary>
        /// Tests UUID generation with version 5 (SHA-1) and namespace for various string samples
        /// </summary>
        [Test]
        public void GenerateUuid_Version5WithNamespace_ProducesConsistentResults()
        {
            var results = new string[TestData.StringSamples.Length];
            
            for (int i = 0; i < TestData.StringSamples.Length; i++)
            {
                results[i] = UuidGenerator.GenerateUuid(TestData.StringSamples[i], TestData.TestNamespaceUuid, 5);
                // Verify the result is a valid UUID format
                Assert.IsTrue(IsValidUuidFormat(results[i]), $"Result for sample {i} is not a valid UUID format: {results[i]}");
                
                // Verify consistency - calling again should produce same result
                var secondCall = UuidGenerator.GenerateUuid(TestData.StringSamples[i], TestData.TestNamespaceUuid, 5);
                Assert.AreEqual(results[i], secondCall, $"UUID generation is not consistent for sample {i}");
            }
        }

        /// <summary>
        /// Tests that the generated UUIDs are platform-compatible
        /// This is a specific test case from the original JavaScript tests
        /// </summary>
        [Test]
        public void GenerateUuid_PlatformCompatible_MatchesExpected()
        {
            var result = UuidGenerator.GenerateUuid("9239107d-259f-4cf8-b62d-0964b680ab08", version: 3);
            Assert.AreEqual("12f01aa4-5090-3f83-b823-7e7cb43246e7", result);
        }

        /// <summary>
        /// Tests default version behavior (should be 5)
        /// </summary>
        [Test]
        public void GenerateUuid_DefaultVersion_UsesVersion5()
        {
            var defaultResult = UuidGenerator.GenerateUuid("Hello world!");
            var version5Result = UuidGenerator.GenerateUuid("Hello world!", version: 5);
            Assert.AreEqual(defaultResult, version5Result);
        }

        /// <summary>
        /// Performance test with very long text (100k+ characters)
        /// This mirrors the "checker of speed of one generation" test from the original
        /// </summary>
        [Test]
        [Category("Performance")]
        public void GenerateUuid_LongText_CompletesWithinReasonableTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = UuidGenerator.GenerateUuid(TestData.LongText);
            stopwatch.Stop();
            
            Assert.IsTrue(IsValidUuidFormat(result));
            
            // Should complete within a reasonable time (e.g., under 1 second)
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, "UUID generation took too long");
        }

        /// <summary>
        /// Tests using reflection to test the private AppendHexByte method
        /// </summary>
        [Test]
        public void AppendHexByte_ByteToHexConversion_ProducesCorrectResults()
        {
            var type = typeof(UuidGenerator);
            var method = type.GetMethod("AppendHexByte", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "AppendHexByte method not found");

            foreach (var testCase in TestData.ByteToHexTestCases)
            {
                var input = (byte)testCase[0];
                var expected = (string)testCase[1];
                var builder = new System.Text.StringBuilder();
                method.Invoke(null, new object[] { builder, input });
                var result = builder.ToString();
                Assert.AreEqual(expected, result, $"AppendHexByte({input}) should return '{expected}' but returned '{result}'");
            }
        }

        /// <summary>
        /// Tests using reflection to test the private AppendHexBytes method
        /// </summary>
        [Test]
        public void AppendHexBytes_ByteArrayToHexConversion_ProducesCorrectResults()
        {
            var type = typeof(UuidGenerator);
            var method = type.GetMethod("AppendHexBytes", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "AppendHexBytes method not found");

            var builder = new System.Text.StringBuilder();
            method.Invoke(null, new object[] { builder, TestData.TestByteArray, 0, TestData.TestByteArray.Length });
            var result = builder.ToString();
            Assert.AreEqual(TestData.TestByteArrayHex, result);
        }

        /// <summary>
        /// Tests using reflection to test the private ValidateUuid method
        /// </summary>
        [Test]
        public void ValidateUuid_ValidUuid_ReturnsTrue()
        {
            var type = typeof(UuidGenerator);
            var method = type.GetMethod("ValidateUuid", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ValidateUuid method not found");

            var result = (bool)method.Invoke(null, new object[] { TestData.TestNamespaceUuid });
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests using reflection to test the private ValidateUuid method with invalid input
        /// </summary>
        [Test]
        public void ValidateUuid_InvalidUuid_ReturnsFalse()
        {
            var type = typeof(UuidGenerator);
            var method = type.GetMethod("ValidateUuid", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ValidateUuid method not found");

            var result = (bool)method.Invoke(null, new object[] { "Lorem ipsum" });
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests using reflection to test the private ParseUuid method
        /// </summary>
        [Test]
        public void ParseUuid_ValidUuid_ReturnsCorrectByteArray()
        {
            var type = typeof(UuidGenerator);
            var method = type.GetMethod("ParseUuid", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ParseUuid method not found");

            var result = (byte[])method.Invoke(null, new object[] { TestData.TestNamespaceUuid });
            Assert.IsNotNull(result);
            Assert.AreEqual(16, result.Length);
            
            // Verify consistency - parsing and re-generating should be consistent
            var hexResult = BitConverter.ToString(result).Replace("-", "").ToLower();
            var expectedHex = TestData.TestNamespaceUuid.Replace("-", "").ToLower();
            Assert.AreEqual(expectedHex, hexResult);
        }

        /// <summary>
        /// Tests using reflection to test the private ParseUuid method with invalid input
        /// </summary>
        [Test]
        public void ParseUuid_InvalidUuid_ThrowsException()
        {
            var type = typeof(UuidGenerator);
            var method = type.GetMethod("ParseUuid", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ParseUuid method not found");

            Assert.Throws<TargetInvocationException>(() =>
            {
                method.Invoke(null, new object[] { "Lorem ipsum" });
            });
        }

        /// <summary>
        /// Tests different parameter overloads work correctly
        /// </summary>
        [Test]
        public void GenerateUuid_DifferentOverloads_WorkCorrectly()
        {
            // Test with just string (should use version 5)
            var result1 = UuidGenerator.GenerateUuid("test");
            var result2 = UuidGenerator.GenerateUuid("test", version: 5);
            Assert.AreEqual(result1, result2);

            // Test with string and namespace (should use version 5)
            var result3 = UuidGenerator.GenerateUuid("test", TestData.TestNamespaceUuid);
            var result4 = UuidGenerator.GenerateUuid("test", TestData.TestNamespaceUuid, 5);
            Assert.AreEqual(result3, result4);

            // Test with string and version
            var result5 = UuidGenerator.GenerateUuid("test", version: 3);
            
            // Test with all parameters
            var result6 = UuidGenerator.GenerateUuid("test", TestData.TestNamespaceUuid, 3);
            
            // All results should be valid UUIDs but different from each other
            Assert.IsTrue(IsValidUuidFormat(result1));
            Assert.IsTrue(IsValidUuidFormat(result3));
            Assert.IsTrue(IsValidUuidFormat(result5));
            Assert.IsTrue(IsValidUuidFormat(result6));
            
            Assert.AreNotEqual(result1, result5); // Version 5 vs Version 3
            Assert.AreNotEqual(result1, result3); // Without namespace vs with namespace
            Assert.AreNotEqual(result5, result6); // Version 3 without namespace vs with namespace
        }

        /// <summary>
        /// Tests that MD5 and SHA-1 produce different results
        /// </summary>
        [Test]
        public void GenerateUuid_DifferentVersions_ProduceDifferentResults()
        {
            const string testString = "test string";
            
            var md5Result = UuidGenerator.GenerateUuid(testString, version: 3);
            var sha1Result = UuidGenerator.GenerateUuid(testString, version: 5);
            
            Assert.AreNotEqual(md5Result, sha1Result);
            Assert.IsTrue(IsValidUuidFormat(md5Result));
            Assert.IsTrue(IsValidUuidFormat(sha1Result));
        }

        /// <summary>
        /// Tests namespace impact on UUID generation
        /// </summary>
        [Test]
        public void GenerateUuid_WithAndWithoutNamespace_ProducesDifferentResults()
        {
            const string testString = "test string";
            
            var withoutNamespace = UuidGenerator.GenerateUuid(testString);
            var withNamespace = UuidGenerator.GenerateUuid(testString, TestData.TestNamespaceUuid);
            
            Assert.AreNotEqual(withoutNamespace, withNamespace);
            Assert.IsTrue(IsValidUuidFormat(withoutNamespace));
            Assert.IsTrue(IsValidUuidFormat(withNamespace));
        }

        /// <summary>
        /// Helper method to validate UUID format
        /// </summary>
        private bool IsValidUuidFormat(string uuid)
        {
            if (string.IsNullOrEmpty(uuid) || uuid.Length != 36)
                return false;

            var parts = uuid.Split('-');
            if (parts.Length != 5)
                return false;

            if (parts[0].Length != 8 || parts[1].Length != 4 || parts[2].Length != 4 || 
                parts[3].Length != 4 || parts[4].Length != 12)
                return false;

            foreach (var part in parts)
            {
                foreach (var ch in part)
                {
                    if (!Uri.IsHexDigit(ch))
                        return false;
                }
            }

            return true;
        }
    }
}