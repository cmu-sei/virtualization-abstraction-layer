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
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using VirtualizationShim;

namespace RavelloShim
{
    public class RavelloVirtualMachine : VirtualMachine
    {
        private string vmId;
        private string applicationId;
        private RestClient restClient;
		private string name;

        //properties
        private VirtualMachineState vmState;

        public RavelloVirtualMachine( RestClient client, string id, string applicationId, string name )
        {
            this.vmId = id;
            this.applicationId = applicationId;
            this.restClient = client;
			this.name = name;
        }
        public void powerOff()
        {
            var request = new RestRequest();
            request.Resource = "/applications/" + applicationId + "/vms/" + vmId + "/stop";
            request.Method = Method.POST;
            IRestResponse response = restClient.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response content: " + vmId + " " + response.Content );
        }

        public void powerOn()
        {
            var request = new RestRequest();
            request.Resource = "/applications/" + applicationId + "/vms/" + vmId + "/start";
            request.Method = Method.POST;
            IRestResponse response = restClient.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response content: " + vmId + " " + response.Content );
        }

        public void restart()
        {
            throw new NotImplementedException();
        }

        public void setOptions( VirtualMachineOptions options )
        {
            throw new NotImplementedException();
        }

        public void shutdown()
        {
            throw new NotImplementedException();
        }

        public string getId()
        {
            return vmId;
        }

		public string getName()
		{
			return name;
		}

        public VirtualMachineState getState()
        {
            update();
            return vmState;
        }

        private void update()
        {
            // todo
            // add a check here that determines how long it has been since the last update.
            // if below a threshold, simply return.

            var request = new RestRequest();
            request.Resource = "/applications/" + applicationId + "/vms/" + vmId + ";deployment";
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            RavelloVm vm = deserial.Deserialize<RavelloVm>( response );

            switch( vm.state )
            {
                case "STARTING":
                case "STARTED":
                    vmState = VirtualMachineState.POWERED_ON;
                    break;
                case "STOPPING":
                case "STOPPED":
                    vmState = VirtualMachineState.POWERED_OFF;
                    break;
                default:
                    vmState = VirtualMachineState.UNKNOWN;
                    break;
            }
            
        }

        public RemoteConsole getConsole()
        {
            update();
            RemoteConsole rc = new RemoteConsole();

            if( getState() != VirtualMachineState.POWERED_ON )
                return rc;
            

            var request = new RestRequest();
            request.Resource = "/applications/" + applicationId + "/vms/" + vmId + "/vncUrl";
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            rc.url = response.Content;

            //RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            //RavelloVm vm = deserial.Deserialize<RavelloVm>( response );


            return rc;
        }
    }
}
