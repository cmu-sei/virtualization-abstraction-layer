using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;
using XenAPI;

namespace XenShim
{
    class XenVirtualMachine : VirtualMachine
    {
        private XenRef<VM> vmRef;
        private VM vm;
        private Session session;

        public XenVirtualMachine( XenRef<VM> vmRef, Session session )
        {
            this.vmRef = vmRef;
            this.session = session;
            this.vm = VM.get_record( session, vmRef );
        }

        public RemoteConsole getConsole()
        {
            throw new NotImplementedException();
        }

        public string getId()
        {
            return vmRef;
        }

		public string getName()
		{
			return "Implement getName() method.";
		}

		public VirtualMachineState getState()
        {
            vm_power_state ps = VM.get_power_state( session, vmRef );

            switch( ps )
            {
                case vm_power_state.Halted:
                    return VirtualMachineState.POWERED_OFF;
                case vm_power_state.Running:
                    return VirtualMachineState.POWERED_ON;
                case vm_power_state.Paused:
                case vm_power_state.Suspended:
                    return VirtualMachineState.SUSPENDED;
                case vm_power_state.unknown:
                default:
                    return VirtualMachineState.UNKNOWN;
            }
        }

        public void powerOff()
        {
            VM.hard_shutdown( session, vmRef );
        }

        public void powerOn()
        {
            VM.start( session, vmRef, false, true );
        }

        public void restart()
        {
            VM.hard_reboot( session, vmRef );
        }

        public void setOptions( VirtualMachineOptions options )
        {
            throw new NotImplementedException();
        }

        public void shutdown()
        {
            VM.shutdown( session, vmRef );
        }
    }
}
