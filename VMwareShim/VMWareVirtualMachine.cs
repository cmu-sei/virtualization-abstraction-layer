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
    class VMWareVirtualMachine : VirtualMachine
    {
        private readonly ManagedObjectReference vmmor;
        private readonly AppUtil.AppUtil cb;
        private readonly string vmId;
        private VirtualMachineState state;

        private VimService _service;
        private ServiceContent _sic;

        public VMWareVirtualMachine( ManagedObjectReference vmmor, AppUtil.AppUtil cb, string vmId )
        {
            this.vmmor = vmmor;
            this.cb = cb;
            this.vmId = vmId;
            this._service = cb.getConnection()._service;
            this._sic = cb.getConnection()._sic;
        }

		public void powerOff()
        {
            ManagedObjectReference taskmor = null;
            taskmor = cb.getConnection().Service.PowerOffVM_Task( vmmor );
        }

        public void powerOn()
        {
            ManagedObjectReference hostmor = cb.getServiceUtil().GetFirstDecendentMoRef( null, "HostSystem" );
            ManagedObjectReference taskmor = null;
            taskmor = cb.getConnection().Service.PowerOnVM_Task( vmmor, hostmor );
        }

        private VirtualMachineMksTicket testMks()
        {
            try
            {
                //ManagedObjectReference hostmor = cb.getServiceUtil().GetFirstDecendentMoRef( null, "HostSystem" );
                VirtualMachineMksTicket ticket = cb.getConnection().Service.AcquireMksTicket( vmmor );
                return ticket;
            }
            catch( SoapException e )
            {
                Console.WriteLine( e.Detail );
                return null;
            }
        }

        public void restart()
        {
            throw new NotImplementedException();
        }

        public void setOptions( VirtualMachineOptions options )
        {
            throw new NotImplementedException();
        }

        public void shutdown()
        {
            throw new NotImplementedException();
        }

        public string getId()
        {
            return vmId;
        }

        public VirtualMachineState getState()
        {
            Object [] properties = getProperties( vmmor, new String [] { "runtime.powerState" } );
            VirtualMachinePowerState vmState = (VirtualMachinePowerState)properties[0];
            switch( vmState )
            {
                case VirtualMachinePowerState.poweredOff:
                    return VirtualMachineState.POWERED_OFF;
                case VirtualMachinePowerState.poweredOn:
                    return VirtualMachineState.POWERED_ON;
                case VirtualMachinePowerState.suspended:
                    return VirtualMachineState.SUSPENDED;
                default:
                    return VirtualMachineState.UNKNOWN;
            }
        }

        public string getName()
        {
            Object [] properties = getProperties( vmmor, new String [] { "name" } );
            return (String)properties[0];
        }

        private String getVmwareId()
        {
            Object [] properties = getProperties( vmmor, new String [] { "name" } );
            return (String)properties [0];
        }



        // copied from PropertyCollector.cs?
        private Object [] getProperties( ManagedObjectReference moRef, String [] properties )
        {
            PropertySpec pSpec = new PropertySpec();
            pSpec.type = moRef.type;
            pSpec.pathSet = properties;
            ObjectSpec oSpec = new ObjectSpec();
            // Set the starting object
            oSpec.obj = moRef;
            PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            pfSpec.propSet = new PropertySpec [] { pSpec };
            pfSpec.objectSet = new ObjectSpec [] { oSpec };
            List<ObjectContent> listobjcontent = retrieveAllProperties( pfSpec );
            ObjectContent [] ocs = listobjcontent.ToArray();
            Object [] ret = new Object [properties.Length];
            if( ocs != null )
            {
                for( int i = 0; i < ocs.Length; ++i )
                {
                    ObjectContent oc = ocs [i];
                    DynamicProperty [] dps = oc.propSet;
                    if( dps != null )
                    {
                        for( int j = 0; j < dps.Length; ++j )
                        {
                            DynamicProperty dp = dps [j];
                            for( int p = 0; p < ret.Length; ++p )
                            {
                                if( properties [p].Equals( dp.name ) )
                                {
                                    ret [p] = dp.val;
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        //also copied from PropertyCollector.cs
        private List<ObjectContent> retrieveAllProperties( PropertyFilterSpec pfSpec )
        {
            List<ObjectContent> listobjcontent = new List<ObjectContent>();
            // RetrievePropertiesEx() returns the properties
            // selected from the PropertyFilterSpec
            RetrieveResult rslts = _service.RetrievePropertiesEx( _sic.propertyCollector, new PropertyFilterSpec [] { pfSpec }, new RetrieveOptions() );
            if( rslts != null && rslts.objects != null && rslts.objects.Length != 0 )
            {
                listobjcontent.AddRange( rslts.objects );
            }
            String token = null;
            if( rslts != null && rslts.token != null )
            {
                token = rslts.token;
            }
            while( token != null && token.Length != 0 )
            {
                rslts = _service.ContinueRetrievePropertiesEx( _sic.propertyCollector, token );
                token = null;
                if( rslts != null )
                {
                    token = rslts.token;
                    if( rslts.objects != null && rslts.objects.Length != 0 )
                    {
                        listobjcontent.AddRange( rslts.objects );
                    }
                }
            }
            return listobjcontent;
        }

        private List<ObjectContent> debugAllProperties()
        {
            List<ObjectContent> listobjcontent = new List<ObjectContent>();
            // RetrievePropertiesEx() returns the properties
            // selected from the PropertyFilterSpec
            RetrieveResult rslts = _service.RetrievePropertiesEx( _sic.propertyCollector, new PropertyFilterSpec [] {  }, new RetrieveOptions() );
            if( rslts != null && rslts.objects != null && rslts.objects.Length != 0 )
            {
                listobjcontent.AddRange( rslts.objects );
            }
            String token = null;
            if( rslts != null && rslts.token != null )
            {
                token = rslts.token;
            }
            while( token != null && token.Length != 0 )
            {
                rslts = _service.ContinueRetrievePropertiesEx( _sic.propertyCollector, token );
                token = null;
                if( rslts != null )
                {
                    token = rslts.token;
                    if( rslts.objects != null && rslts.objects.Length != 0 )
                    {
                        listobjcontent.AddRange( rslts.objects );
                    }
                }
            }
            return listobjcontent;
        }


        public RemoteConsole getConsole()
        {
            RemoteConsole rc = new RemoteConsole();


            VirtualMachineMksTicket ticket = testMks();

            //Console.WriteLine( "Ticket dump:" );
            //Console.WriteLine( "  host: " + ticket.host );
            //Console.WriteLine( "  port: " + ticket.port );
            //Console.WriteLine( "  port specified: " + ticket.portSpecified );
            //Console.WriteLine( "  ssl: " + ticket.sslThumbprint );
            //Console.WriteLine( "  ticket: " + ticket.ticket );
            //Console.WriteLine( "  cfgFile: " + ticket.cfgFile );
            //Console.WriteLine( "  string: " + ticket.ToString() );

            String hostname = "192.168.1.3:9443";
            String serverGuid = "11111111-1111-1111-1111-111111111111";
                   serverGuid = "22222222-2222-2222-2222-222222222222";// TODO do this proceduraly :-|
            String host = "192.168.1.3:443";
            //debugAllProperties();
            String vmId = vmmor.Value;
            String sessionTicket = "cst-VCT-" + ticket.ticket + "--tp-" + ticket.sslThumbprint.Replace( ":", "-" );// TODO find/replace : with -
            String url = "https://" + hostname + "/vsphere-client/webconsole.html?vmId=" + vmId + "&vmName=" + getName() + "&serverGuid=" + serverGuid + "&locale=en_US&host=" + host + "&sessionTicket=" + sessionTicket + "&thumbprint=" + ticket.sslThumbprint;


            //Console.WriteLine( url );

            rc.url = url;
            return rc;
        }
    }
}
