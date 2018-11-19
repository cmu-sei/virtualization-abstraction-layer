using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizationShim
{
    public class VirtualMachineOptions
    {
        private int memory;//size of memory in MB
        private int cpus; // number of CPUs
        private int cores;// number of cores per CPU
        private OperatingSystem os;//operating system running in this vm
    }
}
