using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizationShim
{
    public interface VirtualMachine
    {
        void setOptions( VirtualMachineOptions options );
        void powerOn();
        void powerOff();
        void shutdown();
        void restart();
        string getId();
		string getName();
        VirtualMachineState getState();
        RemoteConsole getConsole();
    }
}
