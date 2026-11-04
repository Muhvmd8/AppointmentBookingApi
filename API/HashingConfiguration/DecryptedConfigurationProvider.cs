using System.Security.Cryptography;
using System.Text;

namespace API.HashingConfiguration
{
    public class DecryptedConfigurationProvider : ConfigurationProvider
    {
        private readonly IConfigurationBuilder _builder;
        private readonly string _encryptionKey;

        public DecryptedConfigurationProvider(IConfigurationBuilder builder)
        {
            _builder = builder;
            _encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")??"";
            if (string.IsNullOrEmpty(_encryptionKey) || (_encryptionKey.Length != 16 && _encryptionKey.Length != 24 && _encryptionKey.Length != 32))
            {
                throw new InvalidOperationException("Encryption key is not set or has an invalid length.");
            }
        }

        public override void Load()
        {
            // Get the initial configuration from the standard sources
            var initialConfig = _builder.Build();

            foreach (var item in initialConfig.AsEnumerable())
            {
                if (item.Value?.StartsWith("ENCRYPTED_DATA:") == true)
                {
                    try
                    {
                        var encryptedValue = item.Value.Substring("ENCRYPTED_DATA:".Length);
                        var decryptedValue = Decrypt(encryptedValue, _encryptionKey);
                        Data[item.Key] = decryptedValue;
                    }
                    catch (Exception ex)
                    {
                        // Handle decryption errors gracefully
                        // You might want to log this error
                        Console.WriteLine($"Error decrypting configuration key '{item.Key}': {ex.Message}");
                        Data[item.Key] = string.Empty; // Set a default value to prevent crashes
                    }
                }
                else
                {
                    Data[item.Key] = item.Value;
                }
            }
        }

        // You need to include the Decrypt method here or in a helper class
        private string Decrypt(string encryptedText, string key)
        {
            // Same decryption logic from your DecryptionService
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16];

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
