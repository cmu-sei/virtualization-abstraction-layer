using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

//using System.Diagnostics;

namespace VMware.Security.CredentialStore {
   internal class CredentialStore : ICredentialStore {
      #region Static members

      private const int s_AquireLockSleepIntervalMilliseconds = 500;
      private const int s_AquireLockTimeoutSeconds = 20;

      private const string s_CredentialElementXPath =
         "/" + s_ViCredentialsElementName + "/" + s_CredentialEntryElementName;

      private const string s_CredentialEntryElementName =
         "passwordEntry";

      private const string s_CredentialsElementXPath =
         "/" + s_ViCredentialsElementName;

      private const string s_HostElementName = "host";
      private const string s_PasswordElementName = "password";
      private const string s_UsernameElementName = "username";
      private const string s_VersionElementName = "version";

      private const string s_VersionElementXPath =
         "/" + s_ViCredentialsElementName + "/" + s_VersionElementName;

      private const string s_ViCredentialsElementName =
         "viCredentials";

      static private readonly string s_DefaultCredentialFilePath =
         @"%APPDATA%\VMware\credstore\vicredentials.xml";

      #endregion Static members

      #region Instance members

      private readonly string _credentialFilePath;

      private bool _objectAlreadyDisposed;

      #endregion Instance members

      static CredentialStore() {
            // Look into the configuration file for a default credentials file path
            string settingFromConfigFile = "";// TODO APC
            //ConfigurationManager.AppSettings["DefaultCredentialStoreFilePath"];
         if (!string.IsNullOrEmpty(settingFromConfigFile)) {
            s_DefaultCredentialFilePath = settingFromConfigFile;
         }
      }

      public CredentialStore()
         : this(
            new FileInfo(
               Environment.ExpandEnvironmentVariables(
                  s_DefaultCredentialFilePath))) {}

      public CredentialStore(FileInfo file) {
         _credentialFilePath = file.FullName;

         if (!file.Directory.Exists) {
            // Create the dir only if it's the default one
            string defaultDir = Path.GetDirectoryName(
               Environment.ExpandEnvironmentVariables(
                  s_DefaultCredentialFilePath));

            if (file.DirectoryName.Equals(
               defaultDir, StringComparison.OrdinalIgnoreCase)) {
               file.Directory.Create();
            } else {
               throw new DirectoryNotFoundException(file.DirectoryName);
            }
         }
      }

      #region ICredentialStore Members

      /// <summary>
      /// Stores the password for a given host and username. If a password
      /// already exists for that host and username, it is overwritten. 
      /// </summary>
      /// <returns><code>true</code> if a password for this host and username
      /// did not already exist</returns>
      /// <exception cref="IOException"/>
      public bool AddPassword(string host, string username, char[] password) {
         if (_objectAlreadyDisposed) {
            throw new ObjectDisposedException("CredentialStore");
         }
         if (string.IsNullOrEmpty(host)) {
            throw new ArgumentException("Host name cannot be empty.", "host");
         }
         if (string.IsNullOrEmpty(username)) {
            throw new ArgumentException(
               "User name cannot be empty.", "username");
         }
         if (password == null) {
            password = new char[0];
         }

         bool result;

         FileStream credentialsFile = null;

         try {
            if (!File.Exists(_credentialFilePath)) {
               // Create the credentials file
               using (
                  File.Create(
                     _credentialFilePath,
                     8192,
                     FileOptions.RandomAccess,
                     GetSecuritySettings())) {}

               credentialsFile = OpenFile(FileShare.None);
               InitializeCredentialsDocument(credentialsFile);
            } else {
               credentialsFile = OpenFile(FileShare.None);
            }

            XmlDocument credentialsXmlDocument =
               LoadCredentialsDocument(credentialsFile);

            // Check if a password exists for this host
            XmlNode credentialNode =
               GetCredentialNode(credentialsXmlDocument, host, username);

            result = (credentialNode == null);

            if (credentialNode == null) {
               credentialNode =
                  credentialsXmlDocument.CreateElement(
                     s_CredentialEntryElementName);
            } else {
               credentialNode.RemoveAll();
            }

            FillCredentialNode(credentialNode, host, username, password);

            // Clear the password so it does not reside in memory in clear text
            Array.Clear(password, 0, password.Length);

            XmlNode credentialsNode =
               credentialsXmlDocument.SelectSingleNode(
                  s_CredentialsElementXPath);

            credentialsNode.AppendChild(credentialNode);

            SaveCredentialsDocument(credentialsXmlDocument, credentialsFile);
         } finally {
            if (credentialsFile != null) {
               credentialsFile.Dispose();
            }
         }

         return result;
      }

      /// <summary>
      /// Removes the password for a given host and username. If no such
      /// password exists, this method has no effect.
      /// </summary>
      /// <returns><code>true</code> if the password existed and was removed
      /// </returns>
      /// <exception cref="IOException"/>
      public bool RemovePassword(string host, string username) {
         if (_objectAlreadyDisposed) {
            throw new ObjectDisposedException("CredentialStore");
         }

         bool result = false;

         if (File.Exists(_credentialFilePath)) {
            FileStream credentialsFile = null;

            try {
               credentialsFile = OpenFile(FileShare.None);

               XmlDocument credentialsXmlDocument =
                  LoadCredentialsDocument(credentialsFile);

               XmlNode nodeToRemove =
                  GetCredentialNode(credentialsXmlDocument, host, username);

               result = (nodeToRemove != null);

               if (nodeToRemove != null) {
                  XmlNode credentials =
                     credentialsXmlDocument.SelectSingleNode(
                        s_CredentialsElementXPath);

                  credentials.RemoveChild(nodeToRemove);

                  SaveCredentialsDocument(
                     credentialsXmlDocument, credentialsFile);
               }
            } finally {
               if (credentialsFile != null) {
                  credentialsFile.Dispose();
               }
            }
         }

         return result;
      }

      /// <summary>
      /// Removes all passwords.
      /// </summary>
      /// <exception cref="IOException"/>
      public void ClearPasswords() {
         if (_objectAlreadyDisposed) {
            throw new ObjectDisposedException("CredentialStore");
         }

         if (File.Exists(_credentialFilePath)) {
            FileStream credentialsFile = null;

            try {
               credentialsFile = OpenFile(FileShare.None);

               XmlDocument credentialsXmlDocument =
                  LoadCredentialsDocument(credentialsFile);

               XmlNode credentialsNode =
                  credentialsXmlDocument.SelectSingleNode(
                     s_CredentialsElementXPath);

               XmlNodeList credentialNodes =
                  credentialsXmlDocument.SelectNodes(s_CredentialElementXPath);

               foreach (XmlNode credentialNode in credentialNodes) {
                  credentialsNode.RemoveChild(credentialNode);
               }

               SaveCredentialsDocument(credentialsXmlDocument, credentialsFile);
            } finally {
               if (credentialsFile != null) {
                  credentialsFile.Dispose();
               }
            }
         }
      }

      /// <summary>
      /// Returns all hosts that have entries in the credential store.
      /// </summary>
      /// <exception cref="IOException"/>
      public IEnumerable<string> GetHosts() {
         if (_objectAlreadyDisposed) {
            throw new ObjectDisposedException("CredentialStore");
         }

         Dictionary<string, string> hosts = new Dictionary<string, string>();

         if (File.Exists(_credentialFilePath)) {
            FileStream credentialsFile = null;

            try {
               credentialsFile = OpenFile(FileShare.Read);

               XmlDocument credentialsXmlDocument =
                  LoadCredentialsDocument(credentialsFile);

               XmlNodeList credentialNodesList =
                  credentialsXmlDocument.SelectNodes(s_CredentialElementXPath);

               foreach (XmlNode credentialNode in credentialNodesList) {
                  if (IsValidPasswordEntryNode(credentialNode)) {
                     string host = credentialNode[s_HostElementName].InnerText;
                     string loweredHost = host.ToLower();

                     // Add the item if it's not alredy in the list 
                     if (!hosts.ContainsKey(loweredHost)) {
                        hosts[loweredHost] = host;
                     }
                  }
               }
            } finally {
               if (credentialsFile != null) {
                  credentialsFile.Dispose();
               }
            }
         }

         return hosts.Keys;
      }

      /// <summary>
      /// Returns all usernames that have passwords stored for a given host.
      /// </summary>
      /// <exception cref="IOException"/>
      public IEnumerable<string> GetUsernames(string host) {
         if (_objectAlreadyDisposed) {
            throw new ObjectDisposedException("CredentialStore");
         }

         List<string> usernames = new List<string>();

         if (File.Exists(_credentialFilePath)) {
            FileStream credentialsFile = null;

            try {
               credentialsFile = OpenFile(FileShare.Read);

               XmlDocument credentialsXmlDocument =
                  LoadCredentialsDocument(credentialsFile);

               XmlNodeList credentialNodes =
                  credentialsXmlDocument.SelectNodes(s_CredentialElementXPath);

               foreach (XmlNode credentialNode in credentialNodes) {
                  if (IsValidPasswordEntryNode(credentialNode)) {
                     string hostEntry =
                        credentialNode[s_HostElementName].InnerText;

                     // Host comparison is case-insensitive
                     if (
                        hostEntry.Equals(
                           host, StringComparison.OrdinalIgnoreCase)) {
                        usernames.Add(
                           credentialNode[s_UsernameElementName].InnerText);
                     }
                  }
               }
            } finally {
               if (credentialsFile != null) {
                  credentialsFile.Dispose();
               }
            }
         }

         return usernames;
      }

      /// <summary>
      /// Gets the password for a given host and username.
      /// </summary>
      /// <returns>The password, or <code>null</code> if none is found
      /// </returns>
      /// <exception cref="IOException"/>
      public char[] GetPassword(string host, string username) {
         if (_objectAlreadyDisposed) {
            throw new ObjectDisposedException("CredentialStore");
         }

         char[] password = null;

         if (File.Exists(_credentialFilePath)) {
            FileStream credentialsFile = null;

            try {
               credentialsFile = OpenFile(FileShare.Read);

               XmlDocument credentialsXmlDocument =
                  LoadCredentialsDocument(credentialsFile);

               password =
                  GetPasswordInternal(credentialsXmlDocument, host, username);
            } finally {
               if (credentialsFile != null) {
                  credentialsFile.Dispose();
               }
            }
         }

         return password;
      }

      #region Disposing

      /// <summary>
      /// Closes this credential store and frees all resources associated with
      /// it. No further <code>ICredentialStore</code> methods may be invoked
      /// on this object.
      /// </summary>
      /// <exception cref="IOException"/>
      public void Close() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      ~CredentialStore() {
         Dispose(false);
      }

      #endregion Disposing

      #endregion

      static private void SaveCredentialsDocument(
         XmlDocument credentialsXmlDocument, Stream credentialFile) {
         credentialFile.Position = 0;
         credentialFile.SetLength(0);
         credentialsXmlDocument.Save(credentialFile);
      }

      static private void FillCredentialNode(
         XmlNode element, string host, string username, char[] password) {
         XmlElement hostElement =
            element.OwnerDocument.CreateElement(s_HostElementName);
         hostElement.InnerText = host;
         element.AppendChild(hostElement);

         XmlElement usernameElement =
            element.OwnerDocument.CreateElement(s_UsernameElementName);
         usernameElement.InnerText = username;
         element.AppendChild(usernameElement);

         XmlElement passElement =
            element.OwnerDocument.CreateElement(s_PasswordElementName);
         passElement.InnerText = ObfuscatePassword(password, host, username);

         element.AppendChild(passElement);
      }

      static private void ValidateCredentialsDocument(
         XmlDocument credentialsXmlDocument) {
         bool valid = true;

         // Do we need to make more validation checks??
         // Maybe we should get a xml schema to validate against

         XmlDeclaration declaration =
            (XmlDeclaration) credentialsXmlDocument.FirstChild;
         valid &= (declaration.Version.StartsWith("1."));
         valid &= (declaration.Encoding == "UTF-8");

         valid &=
            (credentialsXmlDocument.SelectSingleNode(s_CredentialsElementXPath) !=
             null);

         valid &=
            (credentialsXmlDocument.SelectSingleNode(s_VersionElementXPath) !=
             null);

         if (!valid) {
            throw new XmlSchemaValidationException(
               "The credentials .xml file is not well formed.");
         }
      }

      /// <summary>
      /// Returns the credential node which is holding password for this host and username
      /// </summary>
      /// <param name="credentialsXmlDocument"></param>
      /// <param name="host">The host - search is case-insensitive.</param>
      /// <param name="username">The username - search is case-sensitive.</param>
      /// <returns>The node if found, else 'null'.</returns>
      static private XmlNode GetCredentialNode(
         XmlDocument credentialsXmlDocument, string host, string username) {
         XmlNode result = null;

         XmlNodeList passwordEntryNodes =
            credentialsXmlDocument.SelectNodes(s_CredentialElementXPath);

         foreach (XmlNode passwordEntryNode in passwordEntryNodes) {
            if (IsValidPasswordEntryNode(passwordEntryNode)) {
               string hostEntry =
                  passwordEntryNode[s_HostElementName].InnerText;

               // Host comparison is case-insensitive
               if (hostEntry.Equals(host, StringComparison.OrdinalIgnoreCase)) {
                  string usernameEntry =
                     passwordEntryNode[s_UsernameElementName].InnerText;

                  // Username comparison is case-sensitive
                  if (usernameEntry == username) {
                     result = passwordEntryNode;
                     break;
                  }
               }
            }
         }
         return result;
      }

      static private char[] GetPasswordInternal(
         XmlDocument credentialsXmlDocument, string host, string username) {
         char[] result = null;

         XmlNode credentialNode =
            GetCredentialNode(credentialsXmlDocument, host, username);
         if (credentialNode != null) {
            result = UnobfuscatePassword(
               credentialNode[s_PasswordElementName].InnerText,
               host,
               username);
         }

         return result;
      }

      /// <summary>
      /// A password is an arbitrary Unicode string. 
      /// It is case sensitive, and may be empty. 
      /// The value of the password entry is the password obfuscated 
      /// with the following algorithm: 
      /// 1. Let P be the UTF-8 encoding of the password. 
      /// 2. Let N be the size of P in bytes. 
      /// 3. Create a byte buffer B of size max(N+1, 128). 
      /// 4. Copy P followed by a 0 into the beginning of B; fill the rest of B with random bytes. 
      /// 5. Let H be a hash of host and username equal to the value of the Java expression: 
      ///    (host+username).hashCode() % 256 
      /// 6. XOR each element of B with H. 
      /// 7. Base64-encode B and use the resulting string as the value of the entry. 
      /// </summary>
      /// <param name="password"></param>
      /// <param name="host"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      static private string ObfuscatePassword(
         char[] password, string host, string username) {
         /// 1. Let P be the UTF-8 encoding of the password. 
         byte[] passUtf8Encoded = Encoding.UTF8.GetBytes(password);

         /// 2. Let N be the size of P in bytes. 
         int utf8EncodedLength = passUtf8Encoded.Length;

         /// 3. Create a byte buffer B of size max(N+1, 128). 
         byte[] paddedPassword =
            new byte[Math.Max(utf8EncodedLength + 1, 128)];

         /// 4. Copy P followed by a 0 into the beginning of B; 
         /// fill the rest of B with random bytes. 
         Array.Copy(passUtf8Encoded, paddedPassword, utf8EncodedLength);
         paddedPassword[utf8EncodedLength] = 0;
         if (paddedPassword.Length > utf8EncodedLength + 1) {
            Random random = new Random(DateTime.Now.Millisecond);
            byte[] pad =
               new byte[paddedPassword.Length - (utf8EncodedLength + 1)];
            random.NextBytes(pad);
            Array.Copy(
               pad, 0, paddedPassword, utf8EncodedLength + 1, pad.Length);
         }

         /// 5. Let H be a hash of host and username...
         byte hash = (byte) (GetHashCode(host + username) % 256);

         /// 6. XOR each element of B with H.
         for (int i = 0; i < paddedPassword.Length; i++) {
            paddedPassword[i] ^= hash;
         }

         /// 7. Base64-encode B and use the resulting string as the value of the entry. 
         string result = Convert.ToBase64String(paddedPassword);

         return result;
      }

      /// <summary>
      /// Returns a hash code for this string. 
      /// The hash code for a String object is computed as:
      /// s[0]*31^(n-1) + s[1]*31^(n-2) + ... + s[n-1]
      /// using int arithmetic, where:
      /// s[i] is the ith character of the string, 
      /// n is the length of the string, and ^ indicates exponentiation. 
      /// (The hash value of the empty string is zero.) 
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      static private int GetHashCode(string s) {
         if (s == null) {
            throw new ArgumentNullException("s");
         }

         int result = 0;

         for (int i = 0; i < s.Length; i++) {
            result = 31 * result + s[i];
         }

         return result;
      }

      /// <summary>
      /// Unobfuscates password obfuscated with the ObfuscatePassword method
      /// <seealso cref="ObfuscatePassword"/>
      /// </summary>
      static private char[] UnobfuscatePassword(
         string password, string host, string username) {
         byte[] paddedPasswordBytes = Convert.FromBase64String(password);

         byte hash = (byte) (GetHashCode(host + username) % 256);

         for (int i = 0; i < paddedPasswordBytes.Length; i++) {
            paddedPasswordBytes[i] ^= hash;
         }

         int passwordEndIndex = Array.IndexOf<byte>(paddedPasswordBytes, 0);

         if (passwordEndIndex < 0) {
            throw new FormatException(
               "Invalid password format. " +
               string.Format("Host: {0}, Username: {1}", host, username));
         }

         byte[] passwordBytes = new byte[passwordEndIndex];
         Array.Copy(paddedPasswordBytes, passwordBytes, passwordBytes.Length);

         char[] passChar = Encoding.UTF8.GetChars(passwordBytes);

         return passChar;
      }

      static private FileSecurity GetSecuritySettings() {
         FileSecurity security = new FileSecurity();
         security.SetAccessRuleProtection(true, false);
         security.AddAccessRule(
            (FileSystemAccessRule) security.AccessRuleFactory(
                                      new NTAccount(
                                         WindowsIdentity.GetCurrent().Name),
                                      // Full control
                                      -1,
                                      false,
                                      InheritanceFlags.None,
                                      PropagationFlags.None,
                                      AccessControlType.Allow));
         return security;
      }

      static private void InitializeCredentialsDocument(Stream credentialsFile) {
         XmlDocument credentialsXmlDocument = new XmlDocument();

         credentialsXmlDocument.AppendChild(
            credentialsXmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

         XmlElement credentialsElement =
            credentialsXmlDocument.CreateElement(s_ViCredentialsElementName);

         XmlElement versionElement =
            credentialsXmlDocument.CreateElement(s_VersionElementName);
         versionElement.InnerText = "1.0";

         credentialsElement.AppendChild(versionElement);

         credentialsXmlDocument.AppendChild(credentialsElement);

         SaveCredentialsDocument(credentialsXmlDocument, credentialsFile);
      }

      static private XmlDocument LoadCredentialsDocument(Stream credentialsFile) {
         XmlDocument credentialsXmlDocument = new XmlDocument();
         credentialsFile.Position = 0;
         credentialsXmlDocument.Load(credentialsFile);

         ValidateCredentialsDocument(credentialsXmlDocument);

         return credentialsXmlDocument;
      }

      /// <summary>
      /// Tries to open the credentials file using the specified file sharing.
      /// </summary>
      /// <exception cref="TimeoutException">
      /// Thrown if the method cannot open the file in 
      /// s_AquireLockTimeoutSeconds seconds.
      /// </exception>
      private FileStream OpenFile(FileShare fileShare) {
         FileStream result;

         DateTime maxWaitTime =
            DateTime.Now.AddSeconds(s_AquireLockTimeoutSeconds);

         while (true) {
            try {
               if (DateTime.Now > maxWaitTime) {
                  throw new TimeoutException(
                     "Could not aquire access to credential file: " +
                     _credentialFilePath +
                     ". Another process/thread has locked the file.");
               }

               result =
                  File.Open(
                     _credentialFilePath,
                     FileMode.Open,
                     FileAccess.ReadWrite,
                     fileShare);

               break;
            } catch (UnauthorizedAccessException) {
               // Wait for a while and try to lock the file again
               Thread.Sleep(s_AquireLockSleepIntervalMilliseconds);
            }
         }

         return result;
      }

      static private bool IsValidPasswordEntryNode(XmlNode node) {
         bool result = true;
         result &= (node.Name == s_CredentialEntryElementName);
         result &= (node[s_HostElementName] != null);
         result &= (node[s_UsernameElementName] != null);
         result &= (node[s_PasswordElementName] != null);
         return result;
      }

      private void Dispose(bool disposing) {
         if (disposing) {
            // Clean managed resources
         }
         // Clean unmanaged resources

         _objectAlreadyDisposed = true;
      }
   }
}
