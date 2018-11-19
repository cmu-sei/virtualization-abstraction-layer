using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using VirtualizationShim;

namespace VirtualBoxShim
{
    public class VirtualBoxMachineUtility : VirtualMachineUtility
    {
        vboxService service;
        string vboxRef;
        //service

        public VirtualBoxMachineUtility( String username, String password, String hostname )
        {
            //vboxPortTypeClient client;

            service = new vboxService();
            service.Url = hostname;
            //service.IwebsessionManager
            vboxRef = service.IWebsessionManager_logon( "", "" );
            Console.WriteLine( "VirtualBox version: " + service.IVirtualBox_getAPIVersion( vboxRef ) );

        }

        public void disconnect()
        {
            service.IWebsessionManager_logoff( vboxRef );
        }

        public VirtualMachine findVirtualMachine( string criteria )
        {
            try
            {
                //string id = service.IVirtualBox_findMachine( vboxRef, criteria );

                VirtualMachine vm = new VirtualBoxVirtualMachine( service, criteria, vboxRef );
                //service.IMachine_getState( criteria );

                return vm;
            }
            catch( SoapException e )
            {
                throw new VirtualizationShimException( "Cannot find virtual box machine: " + criteria );
            }
        }

        public List<VirtualMachine> listVirtualMachines()
        {
            string [] vms = service.IVirtualBox_getMachines( vboxRef );

            List<VirtualMachine> vmList = new List<VirtualMachine>();

            for( int i = 0; i < vms.Length; i++ )
            {
                string name = service.IMachine_getName( vms [i] );
                vmList.Add( new VirtualBoxVirtualMachine( service, vms[i], vboxRef ) );
                System.Console.WriteLine( i + ": " + vms [i] );
                System.Console.WriteLine( service.IMachine_getState( vms [i] ) );
                System.Console.WriteLine( name );
            }

            return vmList;
        }
    }
}
