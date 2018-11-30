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
using XenAPI;

namespace XenShim
{
    public class XenUtility : VirtualMachineUtility
    {
        private Session session;

        public XenUtility( string hostname, int port, string username, string password )
        {
            session = new Session( hostname, port );

            // Authenticate with username and password. The third parameter tells the server which API version we support.
            session.login_with_password( username, password, API_Version.API_1_3 );

            
        }

        public void disconnect()
        {
            throw new NotImplementedException();
        }

        public VirtualMachine findVirtualMachine( string criteria )
        {
            XenRef<VM> vmRef = new XenRef<VM>( criteria );
            return new XenVirtualMachine( vmRef, session );
        }

        public List<VirtualMachine> listVirtualMachines()
        {
            List<VirtualMachine> vms = new List<VirtualMachine>();

            List<XenRef<VM>> vmRefs = VM.get_all( session );
            foreach( XenRef<VM> vmRef in vmRefs )
            {
                VM vm = VM.get_record( session, vmRef );
                if( vm.is_a_template ) continue;
                if( vm.is_control_domain ) continue;
                System.Console.WriteLine( "Name: {0}\nvCPUs: {1}\nDescription: {2}\n-", vm.name_label, vm.VCPUs_at_startup, vm.name_description );
                vms.Add( findVirtualMachine( vmRef ) );
            }


            return vms;
        }
    }
}
