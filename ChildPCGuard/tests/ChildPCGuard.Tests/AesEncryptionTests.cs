using System;
using System.Security.Cryptography;
using ChildPCGuard.Shared;
using Xunit;

namespace ChildPCGuard.Tests
{
    public class AesEncryptionTests
    {
        private const string TestKey = "TestKeyForAesEncryption12345678901234";
        private const string TestKeyShort = "Short";

        [Fact]
        public void Encrypt_NormalText_ReturnsNonEmptyBase64String()
        {
            var result = AesEncryption.Encrypt("hello world", TestKey);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.DoesNotContain("hello world", result);
        }

        [Fact]
        public void Decrypt_NormalEncryptedText_ReturnsOriginalText()
        {
            string original = "hello world";
            string encrypted = AesEncryption.Encrypt(original, TestKey);
            string decrypted = AesEncryption.Decrypt(encrypted, TestKey);
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void EncryptDecrypt_RoundTrip_PreservesOriginalText()
        {
            string[] testCases = { "test123", "中文测试", "!@#$%^&*()", "", "a" };
            foreach (var testCase in testCases)
            {
                string encrypted = AesEncryption.Encrypt(testCase, TestKey);
                string decrypted = AesEncryption.Decrypt(encrypted, TestKey);
                Assert.Equal(testCase, decrypted);
            }
        }

        [Fact]
        public void Decrypt_WrongKey_ThrowsCryptographicException()
        {
            string encrypted = AesEncryption.Encrypt("hello world", TestKey);
            string wrongKey = "WrongKeyForAesEncryption123456789012";
            Assert.Throws<CryptographicException>(() => AesEncryption.Decrypt(encrypted, wrongKey));
        }

        [Fact]
        public void Encrypt_EmptyText_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => AesEncryption.Encrypt(null!, TestKey));
        }

        [Fact]
        public void Decrypt_InvalidCipherText_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => AesEncryption.Decrypt("not-base64", TestKey));
        }

        [Fact]
        public void ComputeHash_SameInput_ProducesSameHash()
        {
            string input = "password123";
            string hash1 = AesEncryption.ComputeHash(input);
            string hash2 = AesEncryption.ComputeHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ComputeHash_DifferentInput_ProducesDifferentHash()
        {
            string hash1 = AesEncryption.ComputeHash("password1");
            string hash2 = AesEncryption.ComputeHash("password2");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ComputeHash_ReturnsHexadecimalString()
        {
            string hash = AesEncryption.ComputeHash("test");
            Assert.Matches("^[0-9a-fA-F]{64}$", hash);
        }
    }
}
