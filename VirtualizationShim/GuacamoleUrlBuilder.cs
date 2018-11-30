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

using Step.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VirtualizationShim
{
    public class GuacamoleUrlBuilder
    {
        private string guacHost;
        private int guacPort;
        private Crypto crypto = new Crypto( "REDACTED_REDACTED_REDACTED_REDACTED_REDACTED" );

        public GuacamoleUrlBuilder( string guacHost, int guacPort )
        {
            this.guacHost = guacHost;
            this.guacPort = guacPort;
        }

        public string buildUrl( string host, int port, string username, string password )
        {
            string url = "http://" + guacHost + ":" + guacPort + "/#/client/";

            url += encodeRequestedConnectionToken( "" );

            url += "?step-token=";

            url += encodeStepToken( host, port, username, password );

            return url;
        }

        public string encodeStepToken( string host, int port, string username, string password )
        {
            string connectionDetail = String.Format( "id={0}&hostname={1}&username={2}&password={3}", "This_Is_A_Test_Exercise", host, username, password );
            string connectionDetailToken = HttpServerUtility.UrlTokenEncode( Encoding.ASCII.GetBytes( crypto.Encrypt( connectionDetail ) ) );
            return connectionDetailToken;
        }

        public string encodeRequestedConnectionToken( string requestedConnection )
        {
            string plaintext = requestedConnection + "^c^guacamole-auth-step";
            byte [] plainBytes = Encoding.Default.GetBytes( plaintext );
            for( int i = 0; i < plainBytes.Length; i++ )
                if( plainBytes [i] == '^' )
                    plainBytes [i] = 0;

            return Convert.ToBase64String( plainBytes );
        }
    }
}
