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
