using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Microsoft.Web.Services3.Design;
using Vim25Api;
namespace AppUtil
{

    /// <summary>
    /// Connection Handler for WebService
    /// </summary>
    public class SvcConnection
    {

        public enum ConnectionState
        {
            Connected,
            Disconnected,
        }

        public VimService _service;
        protected ConnectionState _state;
        public ServiceContent _sic;
        protected ManagedObjectReference _svcRef;
        public event ConnectionEventHandler AfterConnect;
        public event ConnectionEventHandler AfterDisconnect;
        public event ConnectionEventHandler BeforeDisconnect;
        private bool _ignoreCert;
        public bool ignoreCert
        {
            get { return _ignoreCert; }
            set
            {
                if (value)
                {
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                }
                _ignoreCert = value;
            }
        }
        /// <summary>
        ///  This method is used to validate remote certificate 
        /// </summary>
        /// <param name="sender">string Array</param>
        /// <param name="certificate">X509Certificate certificate</param>
        /// <param name="chain">X509Chain chain</param>
        /// <param name="policyErrors">SslPolicyErrors policyErrors</param>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        public SvcConnection(string svcRefVal)
        {
            _state = ConnectionState.Disconnected;
            if (ignoreCert)
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            }
            _svcRef = new ManagedObjectReference();
            _svcRef.type = "ServiceInstance";
            _svcRef.Value = svcRefVal;
        }


        /// <summary>
        /// Creates an instance of the VMA proxy and establishes a connection
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Connect(string url, string username, string password)
        {
            if (_service != null)
            {
                Disconnect();
            }

            _service = new VimService();
            _service.Url = url;
            _service.Timeout = 600000; //The value can be set to some higher value also.
            _service.CookieContainer = new System.Net.CookieContainer();

            _sic = _service.RetrieveServiceContent(_svcRef);

            if (_sic.sessionManager != null)
            {
                _service.Login(_sic.sessionManager, username, password, null);
            }

            _state = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }

        public void Connect(string url, Cookie cookie)
        {
            if (_service != null)
            {
                Disconnect();
            }
            _service = new VimService();
            _service.Url = url;
            _service.Timeout = 600000; //The value can be set to some higher value also.
            _service.CookieContainer = new System.Net.CookieContainer();
            _service.CookieContainer.Add(cookie);
            _sic = _service.RetrieveServiceContent(_svcRef);

            _state = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }

        public void SaveSession(String fileName, String urlString)
        {
            Cookie cookie = _service.CookieContainer.GetCookies(
                            new Uri(urlString))[0];
            BinaryFormatter bf = new BinaryFormatter();
            Stream s = File.Open(fileName, FileMode.Create);
            bf.Serialize(s, cookie);
            s.Close();
        }




        public void LoadSession(String fileName, String urlString)
        {
            if (_service != null)
            {
                Disconnect();
            }
            _service = new VimService();
            _service.Url = urlString;
            _service.Timeout = 600000;
            _service.CookieContainer = new System.Net.CookieContainer();

            BinaryFormatter bf = new BinaryFormatter();
            Stream s = File.Open(fileName, FileMode.Open);
            Cookie c = bf.Deserialize(s) as Cookie;
            s.Close();
            _service.CookieContainer.Add(c);
            _sic = _service.RetrieveServiceContent(_svcRef);
            _state = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }

        public VimService Service
        {
            get
            {
                return _service;
            }
        }

        public ManagedObjectReference ServiceRef
        {
            get
            {
                return _svcRef;
            }
        }

        public ServiceContent ServiceContent
        {
            get
            {
                return _sic;
            }
        }

        public ManagedObjectReference PropCol
        {
            get
            {
                return _sic.propertyCollector;
            }
        }

        public ManagedObjectReference Root
        {
            get
            {
                return _sic.rootFolder;
            }
        }

        public ConnectionState State
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Disconnects the Connection
        /// </summary>
        public void Disconnect()
        {
            if (_service != null)
            {
                if (BeforeDisconnect != null)
                {
                    BeforeDisconnect(this, new ConnectionEventArgs());
                }

                if (_sic != null)
                    _service.Logout(_sic.sessionManager);

                _service.Dispose();
                _service = null;
                _sic = null;

                _state = ConnectionState.Disconnected;
                if (AfterDisconnect != null)
                {
                    AfterDisconnect(this, new ConnectionEventArgs());
                }
            }
        }

        public void SSOConnect(XmlElement token, string url)
        {
            if (_service != null)
            {
                Disconnect();
            }

            _service = new VimService();
            _service.Url = url;
            _service.Timeout = 600000; //The value can be set to some higher value also.
            _service.CookieContainer = new System.Net.CookieContainer();

            //...
            //When this property is set to true, client requests that use the POST method 
            //expect to receive a 100-Continue response from the server to indicate that 
            //the client should send the data to be posted. This mechanism allows clients 
            //to avoid sending large amounts of data over the network when the server, 
            //based on the request headers, intends to reject the request
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            var customSecurityAssertion = new CustomSecurityAssertionBearer();
            customSecurityAssertion.BinaryToken = token;

            //Setting up the security policy for the request
            Policy policySAML = new Policy();
            policySAML.Assertions.Add(customSecurityAssertion);

            // Setting policy of the service
            //_service.SetPolicy(policySAML); // TODO APC

            _sic = _service.RetrieveServiceContent(_svcRef);

            if (_sic.sessionManager != null)
            {
                _service.LoginByToken(_sic.sessionManager, null);
            }

            _state = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }
    }

    public class ConnectionEventArgs : System.EventArgs
    {
    }

    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);


}
