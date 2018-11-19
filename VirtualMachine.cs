using System;

namespace VirtualizationShim
{
    public interface VirtualMachine
    {
        public VirtualMachine()
        {
        }

        public void powerOn();
        public void powerOff();
        public void shutdown();
        public void restart();
        public void setMemory( int size );
        public void setNumCpu( int quantity );
        public void setCoresPerProcessor( int quantity );
        public void setOperatingSystem( VMOS os );
    }
}