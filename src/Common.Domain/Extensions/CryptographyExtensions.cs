// Copyright (c) 2024 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Security.Cryptography;

namespace Common.Domain.Extensions;

public static class CryptographyExtensions
{
    public static string HashPassword(this string value)
        => BCrypt.Net.BCrypt.HashPassword(value);

    public static bool VerifyHashedPassword(this string hashedPassword, string password)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);

    public static byte[] GenerateSalt(byte[] wrappingKey)
    { 
        var keyToWrap = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var bytes = Convert.FromBase64String(keyToWrap);
        return WrapKey_Aes(bytes, wrappingKey);
    }

    public static byte[] DecryptSalt(this string wrappedKey, byte[] wrappingKey)
    {
        var bytes = Convert.FromBase64String(wrappedKey);
        return UnwrapKey_Aes(bytes, wrappingKey);
    }

    public static string EncryptStringToBase64_Aes(this string plainText, byte[] key)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText));
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException(nameof(key));

        byte[] encrypted;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV();
            var iv = aesAlg.IV;

            aesAlg.Mode = CipherMode.CBC;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

            using var msEncrypt = new MemoryStream();
            msEncrypt.Write(iv, 0, iv.Length);
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            encrypted = msEncrypt.ToArray();
        }

        return Convert.ToBase64String(encrypted);
    }

    public static string DecryptStringFromBase64_Aes(this string cipherTextBase64, byte[] key)
    {
        if (string.IsNullOrEmpty(cipherTextBase64))
            throw new ArgumentNullException(nameof(cipherTextBase64));
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException(nameof(key));

        byte[] cipherText = Convert.FromBase64String(cipherTextBase64);
        string plaintext;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;

            // Extract the IV from the beginning of the ciphertext
            var iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(cipherText, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            aesAlg.Mode = CipherMode.CBC;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv);
            using var msDecrypt = new MemoryStream(cipherText.Skip(iv.Length).ToArray());
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();

        }

        return plaintext;
    }

    private static byte[] WrapKey_Aes(byte[] keyToWrap, byte[] wrappingKey)
    {
        if (keyToWrap == null || keyToWrap.Length <= 0)
            throw new ArgumentNullException(nameof(keyToWrap));
        if (wrappingKey == null || wrappingKey.Length <= 0)
            throw new ArgumentNullException(nameof(wrappingKey));

        byte[] wrappedKey;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = wrappingKey;
            aesAlg.GenerateIV();
            var iv = aesAlg.IV;

            aesAlg.Mode = CipherMode.CBC;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

            using var msEncrypt = new MemoryStream();
            msEncrypt.Write(iv, 0, iv.Length); // Prepend the IV
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(keyToWrap, 0, keyToWrap.Length);
            }
            wrappedKey = msEncrypt.ToArray();
        }

        return wrappedKey;
    }

    private static byte[] UnwrapKey_Aes(byte[] wrappedKey, byte[] wrappingKey)
    {
        if (wrappedKey == null || wrappedKey.Length <= 0)
            throw new ArgumentNullException(nameof(wrappedKey));
        if (wrappingKey == null || wrappingKey.Length <= 0)
            throw new ArgumentNullException(nameof(wrappingKey));

        byte[] unwrappedKey;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = wrappingKey;

            // Extract the IV from the beginning of the wrapped key
            var iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(wrappedKey, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            aesAlg.Mode = CipherMode.CBC;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv);

            using var msDecrypt = new MemoryStream(wrappedKey.Skip(iv.Length).ToArray());
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var msResult = new MemoryStream();
            csDecrypt.CopyTo(msResult);
            unwrappedKey = msResult.ToArray();
        }

        return unwrappedKey;
    }
}
