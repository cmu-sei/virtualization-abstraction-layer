using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizationShim
{
    public class VirtualizationShimException : Exception
    {
        public VirtualizationShimException(){}
        public VirtualizationShimException( string message ) : base( message ){}
        public VirtualizationShimException( string message, Exception inner ) : base( message, inner ){}
    }
}
