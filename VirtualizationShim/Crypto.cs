// Virtualization Abstraction Layer
//
// Copyright 2018 Carnegie Mellon University. All Rights Reserved.
//
// NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
//
// Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
//
// [DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
//
// This Software includes and/or makes use of the following Third-Party Software subject to its own license:
//
// 1. Newtonsoft JSON (https://www.newtonsoft.com/json) Copyright 2007 Newtonsoft.
// 2. RESTSharp (http://restsharp.org/) Copyright 2018 RESTSharp Contributors.
// 3. VMware SDK (https://www.vmware.com/support/developer/vc-sdk/index.html) Copyright 2018 VMWare, Inc..
// 4. VirtualBox SDK (https://www.virtualbox.org/sdkref/) Copyright 2006-2010 Oracle.
// 5. XenServer C# Bindings (https://xenserver.org/open-source-virtualization-download.html) Copyright 1999-2018 Citrix Systems, Inc.
//
// DM18-1224
//

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
