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

using AppUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Vim25Api;
using VirtualizationShim;

namespace VMwareShim
{
    public class VMwareUtility : VirtualMachineUtility
    {
        //HostConnectSpec hostConnectSpec;
        AppUtil.AppUtil cb = null;


        private string [] [] typeInfo = new string [] [] {
              new string[] { "Folder", "name", "childEntity" }, };

        public VMwareUtility( string username, string password, string hostname )
        {
            string [] args = { "--ignorecert", "--disablesso", "--username", username, "--url", hostname, "--password", password, "--typename", "VirtualMachine", "--propertyname", "name" };
            cb = AppUtil.AppUtil.initialize( "Connect", constructOptions(), args );
            cb.connect();

            PrintInventory();

            var serverTime = cb.getConnection().Service.CurrentTime( cb.getConnection().ServiceRef );
            Console.WriteLine( "Got Server Time" );
            Console.WriteLine( serverTime );

        }

        public void disconnect()
        {
            cb.disConnect();
        }
 

        public VirtualMachine findVirtualMachine( string vmName )
        {
            ManagedObjectReference vmmor = cb.getServiceUtil().GetDecendentMoRef( null, "VirtualMachine", vmName );
            if( vmmor == null )
            {
                throw new VirtualizationShimException( "Unable to find virtual machine named : " + vmName + " in Inventory" );
            }

            return new VMWareVirtualMachine( vmmor, cb, vmName );
        }

        public List<VirtualMachine> listVirtualMachines()
        {
            List<VirtualMachine> vms = new List<VirtualMachine>();
            List<string> vmNames = generateListOfVms();

            foreach( string s in vmNames )
                vms.Add( findVirtualMachine( s ) );

            return vms;
        }
 

        private void BuildTypeInfo()
        {
            Console.WriteLine( "Inside BuildTypeInfo function." );
            string usertype = cb.get_option( "typename" );
            string property = cb.get_option( "propertyname" );
            string [] typenprops = new string [2];
            typenprops [0] = usertype;
            typenprops [1] = property;
            typeInfo =
               new string [] [] { typenprops, };
            Console.WriteLine( "Leaving BuildTypeInfo function..." );
        }

        public void PrintInventory()
        {
            try
            {
                Console.WriteLine( "Fetching Inventory" );
                BuildTypeInfo();
                // Retrieve Contents recursively starting at the root folder 
                // and using the default property collector.            
                ObjectContent [] ocary = cb.getServiceUtil().GetContentsRecursively( null, null, typeInfo, true );
                ObjectContent oc = null;
                ManagedObjectReference mor = null;
                DynamicProperty [] pcary = null;
                DynamicProperty pc = null;
                for( int oci = 0; oci < ocary.Length; oci++ )
                {
                    oc = ocary [oci];
                    mor = oc.obj;
                    pcary = oc.propSet;
                    cb.log.LogLine( "Object Type : " + mor.type );
                    cb.log.LogLine( "Reference Value : " + mor.Value );
                    if( pcary != null )
                    {
                        for( int pci = 0; pci < pcary.Length; pci++ )
                        {
                            pc = pcary [pci];
                            cb.log.LogLine( "   Property Name : " + pc.name );
                            if( pc != null )
                            {
                                if( !pc.val.GetType().IsArray )
                                {
                                    cb.log.LogLine( "   Property Value : " + pc.val );
                                }
                                else
                                {
                                    Array ipcary = (Array)pc.val;
                                    cb.log.LogLine( "Val : " + pc.val );
                                    for( int ii = 0; ii < ipcary.Length; ii++ )
                                    {
                                        object oval = ipcary.GetValue( ii );
                                        if( oval.GetType().Name.IndexOf( "ManagedObjectReference" ) >= 0 )
                                        {
                                            ManagedObjectReference imor = (ManagedObjectReference)oval;
                                            cb.log.LogLine( "Inner Object Type : " + imor.type );
                                            cb.log.LogLine( "Inner Reference Value : " + imor.Value );
                                        }
                                        else
                                        {
                                            cb.log.LogLine( "Inner Property Value : " + oval );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                cb.log.LogLine( "Done Printing Inventory" );
                cb.log.LogLine( "Browser : Successful Getting Contents" );
            }

            catch( SoapException e )
            {
                Console.WriteLine( "Browser : Failed Getting Contents" );
                Console.WriteLine( "Encountered SoapException" );
                throw e;
            }
            catch( Exception e )
            {
                cb.log.LogLine( "Browser : Failed Getting Contents" );
                throw e;
            }

        }
        public static OptionSpec [] constructOptions()
        {
            OptionSpec [] useroptions = new OptionSpec [2];
            useroptions [0] = new OptionSpec( "typename", "String", 1
                                            , "Type of managed entity"
                                            , null );
            useroptions [1] = new OptionSpec( "propertyname", "String", 1
                                            , "Name of the Property"
                                            , null );
            return useroptions;
        }


        private List<string> generateListOfVms()
        {
            List<string> vmList = new List<string>();
            try
            {
                Console.WriteLine( "Generating inventory list..." );
                BuildTypeInfo();
                // Retrieve Contents recursively starting at the root folder 
                // and using the default property collector.            
                ObjectContent [] ocary = cb.getServiceUtil().GetContentsRecursively( null, null, typeInfo, true );
                ObjectContent oc = null;
                ManagedObjectReference mor = null;
                DynamicProperty [] pcary = null;
                DynamicProperty pc = null;
                for( int oci = 0; oci < ocary.Length; oci++ )
                {
                    oc = ocary [oci];
                    mor = oc.obj;
                    pcary = oc.propSet;

                    if( !mor.type.Equals( "VirtualMachine" ) )
                        continue;

                    cb.log.LogLine( "Object Type : " + mor.type );
                    cb.log.LogLine( "Reference Value : " + mor.Value );
                    if( pcary != null )
                    {
                        for( int pci = 0; pci < pcary.Length; pci++ )
                        {
                            pc = pcary [pci];
                            if( !pc.name.Equals( "name" ) )
                                continue;

                            cb.log.LogLine( "   Property Name : " + pc.name );
                            if( pc != null )
                            {
                                if( !pc.val.GetType().IsArray )
                                {
                                    vmList.Add( pc.val + "" );
                                    cb.log.LogLine( "   Property Value : " + pc.val );
                                }
                                else
                                {
                                    Array ipcary = (Array)pc.val;
                                    cb.log.LogLine( "Val : " + pc.val );
                                    for( int ii = 0; ii < ipcary.Length; ii++ )
                                    {
                                        object oval = ipcary.GetValue( ii );
                                        if( oval.GetType().Name.IndexOf( "ManagedObjectReference" ) >= 0 )
                                        {
                                            ManagedObjectReference imor = (ManagedObjectReference)oval;
                                            cb.log.LogLine( "Inner Object Type : " + imor.type );
                                            cb.log.LogLine( "Inner Reference Value : " + imor.Value );
                                        }
                                        else
                                        {
                                            cb.log.LogLine( "Inner Property Value : " + oval );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                cb.log.LogLine( "Done Printing Inventory" );
                cb.log.LogLine( "Browser : Successful Getting Contents" );
            }

            catch( SoapException e )
            {
                Console.WriteLine( "Browser : Failed Getting Contents" );
                Console.WriteLine( "Encountered SoapException" );
                throw e;
            }
            catch( Exception e )
            {
                cb.log.LogLine( "Browser : Failed Getting Contents" );
                throw e;
            }

            return vmList;
        }
    }


}
