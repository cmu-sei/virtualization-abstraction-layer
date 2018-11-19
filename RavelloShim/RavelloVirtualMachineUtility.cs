using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;

namespace RavelloShim
{
    public class RavelloVirtualMachineUtility : VirtualMachineUtility
    {
        //private List<string> applicationIds;
        private string applicationId;
        private string username;
        private string password;
        private static string REST_URL = "https://cloud.ravellosystems.com/api/v1/";
        private RestClient restClient;

        public RavelloVirtualMachineUtility( string applicationId, string username, string password )
        {
            this.applicationId = applicationId;
            this.username = username;
            this.password = password;

            this.restClient = getAuthenticatedClient();

            listApplications();
        }

        public void disconnect()
        {
            // do nothing
        }

        public RavelloVirtualMachineUtility()
        {
        }

        public VirtualMachine findVirtualMachine( string criteria )
        {
            try
            {
                var request = new RestRequest();
                request.Resource = "/applications/" + applicationId + "/vms/" + criteria + ";deployment";
                request.Method = Method.GET;
                request.RequestFormat = DataFormat.Json;

                var response = restClient.Execute( request );

                RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

                Console.WriteLine( response.ToString() );

                RavelloVm vm = deserial.Deserialize<RavelloVm>( response );

                if( vm.id == null )
                    throw new VirtualizationShimException( "Machine not found with ID " + criteria );

                RavelloVirtualMachine rvmTemp;
                rvmTemp = new RavelloVirtualMachine( restClient, vm.id + "", applicationId, vm.name );

                return rvmTemp;
            }
            catch( SerializationException e )
            {
                throw new VirtualizationShimException( "Cannot find Ravello machine with ID " + criteria );
            }
        }

        private List<RavelloApplication> listApplications()
        {
            var request = new RestRequest();
            request.Resource = "/applications";
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            List<RavelloApplication> apps = deserial.Deserialize<List<RavelloApplication>>( response );

            foreach( var a in apps )
            {
                Console.WriteLine( a.id + " " + a.name );
            }

            return apps;
        }

        public List<VirtualMachine> listVirtualMachines()
        {
            var request = new RestRequest();
            request.Resource = "/applications/" + applicationId + "/vms";
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            var response = restClient.Execute( request );

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            List<RavelloVm> vms = deserial.Deserialize<List<RavelloVm>>( response );

            List<VirtualMachine> rvms = new List<VirtualMachine>();

            RavelloVirtualMachine rvmTemp;
            foreach( var rvm in vms )
            {
                rvmTemp = new RavelloVirtualMachine( restClient, rvm.id + "", applicationId, rvm.name );
                rvms.Add( rvmTemp );
            }

            return rvms;
        }

		public void changeApplication( string applicationId )
		{
			this.applicationId = applicationId;
		}

        private RestClient getAuthenticatedClient()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri( REST_URL );
            client.Authenticator = new HttpBasicAuthenticator( username, password );

            var request = new RestRequest();
            request.Resource = "/login";
            request.Method = Method.POST;

            IRestResponse response = client.Execute( request );

            System.Diagnostics.Debug.WriteLine( "Response code for login is " + response.StatusCode );

            return client;
        }


		public List<RavelloBlueprint> listBlueprints()
		{
			var request = new RestRequest();
			request.Resource = "/blueprints";
			request.Method = Method.GET;
			request.RequestFormat = DataFormat.Json;

			var response = restClient.Execute( request );

			//RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

			//Console.WriteLine( "Blah " + response.Content );
			//List<RavelloVm> vms = deserial.Deserialize<List<RavelloVm>>( response );

			//List<DiskImage> test = deserial.Deserialize<List<DiskImage>>( response );

			var deserialized = JsonConvert.DeserializeObject<List<RavelloBlueprint>>( response.Content );

			
			foreach( var rbp in deserialized )
			{
				Console.WriteLine( rbp.id + " " + rbp.name );
				
			}

			return deserialized;
			//return rvms;
		}

		public RavelloBlueprint getBlueprint( long id )
		{
			var request = new RestRequest();
			request.Resource = "/blueprints/" + id;
			request.Method = Method.GET;
			request.RequestFormat = DataFormat.Json;

			var response = restClient.Execute( request );

			var bp = JsonConvert.DeserializeObject<RavelloBlueprint>( response.Content );

			return bp;

		}
		// For Ravello Lab Deploy Demo

		public void listDiskImages()
		{
			var request = new RestRequest();
			request.Resource = "/diskImages";
			request.Method = Method.GET;
			request.RequestFormat = DataFormat.Json;

			var response = restClient.Execute( request );

			//RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

			//Console.WriteLine( "Blah " + response.Content );
			//List<RavelloVm> vms = deserial.Deserialize<List<RavelloVm>>( response );

			//List<DiskImage> test = deserial.Deserialize<List<DiskImage>>( response );

			var deserialized = JsonConvert.DeserializeObject<List<DiskImage>>( response.Content );

			//Console.WriteLine( "Disk image info: " + deserialized.ElementAt(4 ).name + " " + deserialized.ElementAt(4).id );


			//List<VirtualMachine> rvms = new List<VirtualMachine>();

			//RavelloVirtualMachine rvmTemp;
			//foreach( var rvm in vms )
			//{
			//	rvmTemp = new RavelloVirtualMachine( restClient, rvm.id + "", applicationId );
			//	rvms.Add( rvmTemp );
			//}

			//return rvms;
		}

		public void publishApplication( long applicationId )
		{
			var request = new RestRequest();
			request.Resource = "/applications/" + applicationId + "/publish";
			request.Method = Method.POST;
			request.RequestFormat = DataFormat.Json;

			
			

			

			request.AddJsonBody( new PublishApplicationPerformance() );

			var response = restClient.Execute( request );
			Console.WriteLine( "Publish response: " + response.StatusCode );

			var body = request.Parameters.Where( p => p.Type == ParameterType.RequestBody ).FirstOrDefault();
			if( body != null )
			{
				Console.WriteLine( "publish request body: {0}", body.Value );
			}
		}

		public void addVmToApplication( long applicationId, long baseVmId )
		{
			var request = new RestRequest();
			request.Resource = "/applications/" + applicationId + "/vms";
			request.Method = Method.POST;
			request.RequestFormat = DataFormat.Json;

			//Dictionary<string, long> addVm = new Dictionary<string, long>();
			RavelloVm addVm = new RavelloVm();

			addVm.baseVmId = baseVmId;

			request.AddJsonBody( addVm );

			var response = restClient.Execute( request );

			var body = request.Parameters.Where( p => p.Type == ParameterType.RequestBody ).FirstOrDefault();
			if( body != null )
			{
				Console.WriteLine( "add vm request body: {0}", body.Value );
			}
			Console.WriteLine( "add vm to application response: " + response.Content );

			//var deserialized = JsonConvert.DeserializeObject<Application>( response.Content );
		}

		public Application createApplicationFromBlueprint( RavelloBlueprint bp, String name )
		{

			Application app = new Application();
			app.name = "corndemo-" + name;
			app.description = "A lab deployment demo test. Made from blueprint: " + bp.id;
			app.baseBlueprintId = bp.id;

			var request = new RestRequest();
			request.Resource = "/applications";
			request.Method = Method.POST;
			request.RequestFormat = DataFormat.Json;

			request.AddJsonBody( app );

			var response = restClient.Execute( request );

			Console.WriteLine( "Application creation response: " + response.Content );

			var deserialized = JsonConvert.DeserializeObject<Application>( response.Content );

			Console.WriteLine( "New application created. ID: " + deserialized.id );
			return deserialized;
		}

		public Application createApplication( string name )
		{
			HardDrive hd = new HardDrive();
			//hd.baseDiskImageId = 78020621;
			hd.name = "apc windows 2003 disk";
			hd.boot = true;
			hd.controller = "lsi53c1030";
			hd.controllerIndex = 0;
			hd.controllerPciSlot = 1;
			hd.imageFetchMode = "LAZY";
			

			Size vmMemSize = new Size();
			vmMemSize.unit = SizeUnit.GB;
			vmMemSize.value = 2;

			RavelloVm vm = new RavelloVm();
			//vm.name = "babys first vm 001";
			vm.baseVmId = 79603141;
			//vm.description = "see name";
			//vm.loadingPercentage = 100;
			//vm.loadingStatus = LoadingStatus.DONE;
			//vm.vmOrderGroupId = 1;
			//vm.stopTimeOut = 600;
			//vm.powerOffOnStopTimeOut = true;
			//vm.memorySize = vmMemSize;
			//vm.numCpus = 1;
			//vm.hardDrives.Add( hd );

			VMOrderGroup vmOrderGroup = new VMOrderGroup();
			vmOrderGroup.id = 1;
			vmOrderGroup.name = "group1";
			vmOrderGroup.order = 0;
			vmOrderGroup.delay = 0;

			Design design = new Design();
			//design.vms.Add( vm );
			//design.vmOrderGroups.Add( vmOrderGroup );

			Application app = new Application();
			app.name = "corndemo-" + name;
			app.description = "A lab deployment demo test.";
			app.design = design;

			var request = new RestRequest();
			request.Resource = "/applications";
			request.Method = Method.POST;
			request.RequestFormat = DataFormat.Json;

			request.AddJsonBody( app );

			var response = restClient.Execute( request );

			Console.WriteLine( "Application creation response: " + response.Content );

			var deserialized = JsonConvert.DeserializeObject<Application>( response.Content );
			
			Console.WriteLine( "New application created. ID: " + deserialized.id );
			return deserialized;
		}
    }
}
