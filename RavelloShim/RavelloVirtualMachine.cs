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
