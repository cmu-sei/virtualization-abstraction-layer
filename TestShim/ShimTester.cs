using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;
using RavelloShim;
using VMwareShim;
using KimchiShim;
using System.IO;
using System.Reflection;
using VirtualBoxShim;
using XenShim;

namespace TestShim
{
    class ShimTester
    {
        static void Main( string [] args )
        {
            //GuacamoleUrlBuilder gub = new GuacamoleUrlBuilder( "192.168.1.10", 80 );

            //Console.WriteLine( gub.buildUrl( "192.168.1.196", 3389, "apcorn", "password" ) );
            //XenUtility xenUtil = new XenUtility( "192.168.1.4", 80, "root", "password" );

            //List<VirtualMachine> vms = xenUtil.listVirtualMachines();

            //vms.First().powerOn();
            // open config file
            //string path = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), @"config.properties" );

            //var data = new Dictionary<string, string>();
            //foreach( var row in File.ReadAllLines( path ) )
            //    data.Add( row.Split( '=' ) [0], string.Join( "=", row.Split( '=' ).Skip( 1 ).ToArray() ) );

            //Console.WriteLine(  );
            //Console.WriteLine(  );

            //System.Console.WriteLine( "Gonna try connecting to virtualbox..." );
            //string vmid = "ubuntu vbox server";
            //VirtualMachineUtility util = new VirtualBoxMachineUtility( "", "", "http://192.168.1.45:22" );
            //util.listVirtualMachines();

            //VirtualMachine vboxMachine = util.findVirtualMachine( vmid );
           // System.Console.ReadKey();
            //vboxMachine.powerOff();
            //vboxMachine.powerOn();

            //VirtualMachineUtility kimchUtil = new KimchiVirtualMachineUtility( data["kimchi.host"], data["kimchi.username"], data["kimchi.password"] );// TODO create new user, or change apcorn's password
            //VirtualMachineUtility vmwareUtil = new VMwareUtility( "administrator@vsphere.local", "", "https://192.168.1.8/sdk" );
            //VirtualMachineUtility util = new VMwareUtility( "root", "", "https://vmware.local/sdk" );// old esxi connect string
            //VirtualMachineUtility util2 = new RavelloVirtualMachineUtility( "12345678", "", "" );


            //util.listVirtualMachines();

            //VirtualMachine kvm = util.findVirtualMachine( "ubuntu-test-server" );
            //Console.WriteLine( kvm.getId() );
            //Console.WriteLine( kvm.getConsole().url );
            //kvm.powerOn();
            //Console.ReadKey();
            //kvm.powerOff();
            //VirtualMachine vm = util2.findVirtualMachine( "1240034346716696" );
            //VirtualMachine vm = util.findVirtualMachine( "apcorn test vm" );

            //vm.getConsole();

            //Console.WriteLine( "Virtual machine name: " + vm.getId() );
            //Console.WriteLine( "State: " + vm.getState() );
            //vm.powerOn();
            //Console.ReadKey();

            //Console.WriteLine( vm.getConsole().url );


            //util.disconnect();
            //System.Console.ReadKey();


            // Screenshot code

            //VirtualMachineUtility kimchUtil = new KimchiVirtualMachineUtility( "kimchihost", "hqbovik", "password" );
            //VirtualMachineUtility vmwareUtil = new VMwareUtility( "administrator@vsphere.local", "password", "https://vmwarehost/sdk" );
            //VirtualMachineUtility ravelloUtil = new RavelloVirtualMachineUtility( "12345678", "hqbovik", "password" );
            //VirtualMachineUtility vboxUtil = new VirtualBoxMachineUtility( "", "", "http://vboxhost" );
            //VirtualMachineUtility xenUtil = new XenUtility( "xenhost", 80, "hqbovik", "password" );




			//List<VirtualMachineUtility> utilities = new List<VirtualMachineUtility>();

			//utilities.Add( new KimchiVirtualMachineUtility( "kimchihost", "hqbovik", "password" ) );
			//utilities.Add( new VMwareUtility( "administrator@vsphere.local", "password", "https://vmwarehost/sdk" ) );
			RavelloVirtualMachineUtility rvmu = new RavelloVirtualMachineUtility( "12345678", "user@example.com", "password" );
			//utilities.Add( new VirtualBoxMachineUtility( "", "", "http://vboxhost" ) );
			//utilities.Add( new XenUtility( "xenhost", 80, "hqbovik", "password" ) );




			Console.WriteLine( "okay" );
			//rvmu.listDiskImages();
			//Application app = rvmu.createApplication( "alexdemo031" );

			rvmu.listBlueprints();

			System.Console.ReadKey();

			RavelloBlueprint bp = new RavelloBlueprint();
			bp.id = 12345678;

			rvmu.createApplicationFromBlueprint( bp, "apc-test-bp-0000" );

			//	rvmu.addVmToApplication( app.id, 12345678 );
			//	rvmu.addVmToApplication( app.id, 12345679 );
			//	rvmu.addVmToApplication( app.id, 12345670 );
			System.Console.ReadKey();
			//rvmu.publishApplication( app.id );
			System.Console.ReadKey();
		}
	}
}
