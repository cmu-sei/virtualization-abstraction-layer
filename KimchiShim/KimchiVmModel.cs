using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KimchiShim
{
    public class KimchiVmList
    {
        public List<KimchiVmContainer> ravelloVms;
    }

    public class KimchiVmContainer
    {
        public KimchiVm ravelloVm;
    }

    public class KimchiVm
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string state { get; set; }
    }
}
