using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualizationShim;

namespace VirtualBoxShim
{
    class VirtualBoxVirtualMachine : VirtualMachine
    {
        vboxService service;
        string machineRef;
        string vboxRef;

        public VirtualBoxVirtualMachine( vboxService service, string machineRef, string vboxRef )
        {
            this.service = service;
            this.machineRef = machineRef;
            this.vboxRef = vboxRef;
            getState();// this forces an attempt to hit the server. if the machine doesnt exist, an exception will be thrown
        }

        public RemoteConsole getConsole()
        {
            return new RemoteConsole();
        }

        public string getId()
        {
            return machineRef;
        }

		public string getName()
		{
			return "Implement getName() method.";
		}

		public VirtualMachineState getState()
        {
            MachineState ms = service.IMachine_getState( machineRef );

            switch( ms )
            {
                case MachineState.Running:
                    return VirtualMachineState.POWERED_ON;
                case MachineState.PoweredOff:
                    return VirtualMachineState.POWERED_OFF;
                case MachineState.Paused:
                    return VirtualMachineState.SUSPENDED;
            }

            return VirtualMachineState.UNKNOWN;
        }

        public void powerOff()
        {
            //string sessionRef = service.IMachine_getSessionPID( machineRef );
            //service.IMachine_lockMachine( machineRef, sessionRef, LockType.Shared );
            //string sessionName = service.IMachine_getSessionName( machineRef );
            string session = service.IWebsessionManager_getSessionObject( vboxRef );

            service.IMachine_lockMachine( machineRef, session, LockType.Shared );

            //service.IWebsessionM
            //service.IConsole_powerDown( machineRef );
            string console = service.ISession_getConsole( session );
            service.IConsole_powerDown( console );
            service.ISession_unlockMachine( session );
        }

        public void powerOn()
        {
            string session = service.IWebsessionManager_getSessionObject( vboxRef );

            //service.IMachine_lockMachine( machineRef, session, LockType.Shared );
            service.IMachine_launchVMProcess( machineRef, session, "", "" );
            //string console = service.ISession_getConsole( session );
            //service.IConsole_powerUp( console );
            //service.ISession_unlockMachine( session );
        }

        public void restart()
        {
            service.IConsole_reset( machineRef );
        }

        public void setOptions( VirtualMachineOptions options )
        {
            throw new NotImplementedException();
        }

        public void shutdown()
        {
            service.IConsole_powerButton( machineRef );
        }
    }
}
