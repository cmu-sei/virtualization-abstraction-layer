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
