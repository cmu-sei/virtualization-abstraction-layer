using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Step.Common
{
    public class Crypto
    {
        public Crypto(string key)
        {
            _aes = new AesCryptoServiceProvider
            {
                Key = Encoding.ASCII.GetBytes(key),
            };
        }

        private AesCryptoServiceProvider _aes = null;

        #region Encrypt

        public string Encrypt(string plain)
        {
            _aes.GenerateIV();

            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, _aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(Encoding.ASCII.GetBytes(plain), 0, Encoding.ASCII.GetByteCount(plain));
                cryptoStream.Flush();
                cryptoStream.Close();
                byte[] bytes = memoryStream.ToArray();
                string token = Convert.ToBase64String(bytes);
                string iv = Convert.ToBase64String(_aes.IV);
                return String.Format("{0} | {1}", token, iv);
            }
        }

        public string Encrypt(byte[] plain)
        {
            return Encrypt(Encoding.ASCII.GetString(plain));
        }

        #endregion

        #region Decrypt

        public string Decrypt(string input)
        {
            string decrypted = "";
            int x = input.IndexOf('|');
            if (x > 0)
            {
                string token = input.Substring(0, x).Trim();
                byte[] bytes = Convert.FromBase64String(token);

                string iv = input.Substring(x + 1).Trim();
                _aes.IV = Convert.FromBase64String(iv);

                using (MemoryStream memoryStream = new MemoryStream(bytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, _aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    byte[] bf = new byte[bytes.Length];
                    int count = cryptoStream.Read(bf, 0, bf.Length);
                    decrypted = Encoding.ASCII.GetString(bf, 0, count);
                }
            }
            return decrypted;
        }

        public string Decrypt(byte[] cipher)
        {
            return Decrypt(Encoding.ASCII.GetString(cipher));
        }

        #endregion

        #region Hash

        /// <summary>
        /// Generate a hash for the provided string
        /// </summary>
        /// <param name="salt">random string used to modify the hash</param>
        /// <param name="password">plain text data to hash</param>
        /// <returns>A byte stream representing the hash output</returns>
        /// 
        public static byte[] Hash(string salt, string data)
        {
            // create a string that combines the provided data and the salt
            string saltedValue = salt + data;

            // convert the string into a byte array for hashing
            byte[] byteValue = Encoding.UTF8.GetBytes(saltedValue);

            // compute the hashed value
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] hashedInput = sha.ComputeHash(byteValue);

            // return the string representation of the data
            return hashedInput;
        }       

        #endregion
    }
}
