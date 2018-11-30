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

using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;

namespace KimchiShim
{
    public class KimchiVirtualMachineUtility : VirtualMachineUtility
    {
        private string username;
        private string password;
        private string host;
        private RestClient restClient;

        public KimchiVirtualMachineUtility( string host, string username, string password )
        {
            this.username = username;
            this.password = password;
            this.host = host;

            initClient();
        }

        public void disconnect()
        {
            // do nothing
        }

        public VirtualMachine findVirtualMachine( string criteria )
        {
            var request = new RestRequest();
            request.Resource = "/vms/" + criteria;
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            KimchiVm vm = deserial.Deserialize<KimchiVm>( response );

            if( vm.name == null || vm.name == "null" )
            {
                throw new VirtualizationShimException( "Kimchi KVM machine not found." );
            }

            KimchiVirtualMachine kvmTemp;
            kvmTemp = new KimchiVirtualMachine( restClient, vm.name );

            return kvmTemp;
        }

        public List<VirtualMachine> listVirtualMachines()
        {
            var request = new RestRequest();
            request.Resource = "/vms/";
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            List<KimchiVm> vms = deserial.Deserialize<List<KimchiVm>>( response );

            List<VirtualMachine> rvms = new List<VirtualMachine>();

            KimchiVirtualMachine rvmTemp;
            foreach( var rvm in vms )
            {
                Console.WriteLine( "KVM machine: " + rvm.name );
                rvmTemp = new KimchiVirtualMachine( restClient, rvm.name );
                rvms.Add( rvmTemp );
            }

            return rvms;
        }

        private void initClient()
        {
            restClient = new RestClient();
            restClient.BaseUrl = new Uri( host + "/plugins/kimchi" );
            restClient.Authenticator = new HttpBasicAuthenticator( username, password );

            System.Net.ServicePointManager.ServerCertificateValidationCallback += ( sender, certificate, chain, errors ) => true;
        }
    }
}
