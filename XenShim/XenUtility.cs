using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;
using XenAPI;

namespace XenShim
{
    public class XenUtility : VirtualMachineUtility
    {
        private Session session;

        public XenUtility( string hostname, int port, string username, string password )
        {
            session = new Session( hostname, port );

            // Authenticate with username and password. The third parameter tells the server which API version we support.
            session.login_with_password( username, password, API_Version.API_1_3 );

            
        }

        public void disconnect()
        {
            throw new NotImplementedException();
        }

        public VirtualMachine findVirtualMachine( string criteria )
        {
            XenRef<VM> vmRef = new XenRef<VM>( criteria );
            return new XenVirtualMachine( vmRef, session );
        }

        public List<VirtualMachine> listVirtualMachines()
        {
            List<VirtualMachine> vms = new List<VirtualMachine>();

            List<XenRef<VM>> vmRefs = VM.get_all( session );
            foreach( XenRef<VM> vmRef in vmRefs )
            {
                VM vm = VM.get_record( session, vmRef );
                if( vm.is_a_template ) continue;
                if( vm.is_control_domain ) continue;
                System.Console.WriteLine( "Name: {0}\nvCPUs: {1}\nDescription: {2}\n-", vm.name_label, vm.VCPUs_at_startup, vm.name_description );
                vms.Add( findVirtualMachine( vmRef ) );
            }


            return vms;
        }
    }
}
