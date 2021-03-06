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
