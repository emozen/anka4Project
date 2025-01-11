using System.Security.Cryptography;
using System.Text;

namespace SimurgWeb.Utility
{
    public class EncryptionHelper
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes("987654321");
        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                GetKeyToByte();
                aes.Key = keyBytes;
                aes.IV = new byte[16]; // Varsayılan IV: Sıfırlarla dolu

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                GetKeyToByte();
                aes.Key = keyBytes;
                aes.IV = new byte[16];

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public void GetKeyToByte()
        {
            if (keyBytes.Length < 16)
                Array.Resize(ref keyBytes, 16); // Kısa ise doldur
            else if (keyBytes.Length > 32)
                keyBytes = keyBytes.Take(32).ToArray(); // Uzun ise kes
        }
    }
}
