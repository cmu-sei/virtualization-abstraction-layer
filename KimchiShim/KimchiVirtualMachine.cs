using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;
using RestSharp;
using RestSharp.Authenticators;

namespace KimchiShim
{
    class KimchiVirtualMachine : VirtualMachine
    { 
        private string vmId;
        private string applicationId;
        private RestClient restClient;

        //properties
        private VirtualMachineState vmState;

        public KimchiVirtualMachine( RestClient client, string id )
        {
            this.vmId = id;
            this.restClient = client;
        }
        public void powerOff()
        {
            var request = new RestRequest();
            request.Resource = "/vms/" + vmId + "/poweroff";
            request.Method = Method.POST;
            IRestResponse response = restClient.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response content: " + vmId + " " + response.Content );
        }

		public string getName()
		{
			return "Implement getName() method.";
		}

        public void powerOn()
        {
            var request = new RestRequest();
            request.Resource = "/vms/" + vmId + "/start";
            request.Method = Method.POST;
            IRestResponse response = restClient.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response content: " + vmId + " " + response.Content );
        }

        public void restart()
        {
            var request = new RestRequest();
            request.Resource = "/vms/" + vmId + "/reset";
            request.Method = Method.POST;
            IRestResponse response = restClient.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response content: " + vmId + " " + response.Content );
        }

        public void setOptions( VirtualMachineOptions options )
        {
            throw new NotImplementedException();
        }

        public void shutdown()
        {
            var request = new RestRequest();
            request.Resource = "/vms/" + vmId + "/shutdown";
            request.Method = Method.POST;
            IRestResponse response = restClient.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response content: " + vmId + " " + response.Content );
        }

        public string getId()
        {
            return vmId;
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
            request.Resource = "/vms/" + vmId;
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            KimchiVm vm = deserial.Deserialize<KimchiVm>( response );

            switch( vm.state )
            {
                case "running":
                    vmState = VirtualMachineState.POWERED_ON;
                    break;
                case "shutoff":
                    vmState = VirtualMachineState.POWERED_OFF;
                    break;
                default:
                    vmState = VirtualMachineState.UNKNOWN;
                    break;
            }

        }

        public RemoteConsole getConsole()
        {
            //update();
            //RemoteConsole rc = new RemoteConsole();

            //if( getState() != VirtualMachineState.POWERED_ON )
            //    return rc;


            //var request = new RestRequest();
            //request.Resource = "/applications/" + applicationId + "/vms/" + vmId + "/vncUrl";
            //request.Method = Method.GET;
            //request.RequestFormat = DataFormat.Json;

            //var response = restClient.Execute( request );

            //rc.url = response.Content;

            ////RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            ////RavelloVm vm = deserial.Deserialize<RavelloVm>( response );


            //return rc;
            String token = System.Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( getId() ) );
            token = token.Replace( "=", "" );
            token = token.Replace( "-", "+" );
            token = token.Replace( "_", "/" );

            String url = String.Format( "https://kimchiserver:8001/plugins/kimchi/novnc/vnc_auto.html?port=8001&path=websockify?token={0}&encrypt=1", token );
            RemoteConsole rc = new RemoteConsole();
            rc.url = url;
            return rc;
        }
    }
}
