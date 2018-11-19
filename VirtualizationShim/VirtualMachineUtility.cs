using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizationShim
{
    public interface VirtualMachineUtility
    {
        List<VirtualMachine> listVirtualMachines();
        VirtualMachine findVirtualMachine( string criteria );
        void disconnect();
    }
}
