using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelloShim
{
    // Everything in here is used only to deserialize VM data from the Ravello API

    public class RavelloApplicationList
    {
        public List<RavelloApplicationContainer> ravelloVms;
    }

    public class RavelloApplicationContainer
    {
        public RavelloApplication ravelloApplication;
    }

    public class RavelloApplication
    {
        public long id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
    }
}
