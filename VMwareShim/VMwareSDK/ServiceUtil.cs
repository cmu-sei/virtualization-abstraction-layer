using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Vim25Api;
using System.Xml;
namespace AppUtil
{

    /// <summary>
    /// Utility wrapper methods for the vimService methods
    /// </summary>
    public class ServiceUtil
    {

        private AppUtil _ci;
        private SvcConnection _connection;

        public ServiceUtil()
        {
        }

        public void Init(AppUtil ci)
        {
            _ci = ci;
            _connection = ci.getConnection();
        }

        /// <summary>
        /// Connect to the service
        /// </summary>
        public void ClientConnect()
        {
            if (_ci.isSSODisabled())
            {
                ClientConnectDeprecated();
            }
            else
            {
                ClientConnectSSO();
            }

        }
        /// <summary>
        /// Connect to the service
        /// </summary>
        public void ClientConnectSSO()
        {
            try
            {
                string url = _ci.getServiceUrl();
                string ssoUrl = _ci.getSsoServiceUrl();
                string username = _ci.getUsername();
                string password = _ci.getPassword();

                if (ssoUrl != null && username != null && password != null)
                {
                    //get bearer token from SSO server
                    
                    var xmlBearerToken =
                        AcquireBearerTokenByUserCredentialSample.AcquireBearerTokenByUserCredential.GetToken(
                        new string[] { ssoUrl, username, password });

                    if (xmlBearerToken != null)
                        Console.WriteLine("Successfully acquired Bearer token from '{0}'...", ssoUrl);

                    //login using bearer token
                    Console.WriteLine("Connecting...");
                    _connection.SSOConnect(xmlBearerToken, url);
                }
                else
                {
                    throw new ArgumentHandlingException("Missing Arguement: url/username/password");
                }
            }
            catch (Exception e)
            {
                _ci.getUtil().LogException(e);
                throw e;
            }
        }

        /// <summary>
        /// Connect to the service
        /// </summary>
        public void ClientConnectDeprecated()
        {
            try
            {
                string url = _ci.getServiceUrl();
                string username = _ci.getUsername();
                string password = _ci.getPassword();
                if (url != null && username != null && password != null)
                {
                    _connection.Connect(url, username, password);
                }
                else
                {
                    throw new ArgumentHandlingException("Missing Arguement: url/username/password");
                }
            }
            catch (Exception e)
            {
                _ci.getUtil().LogException(e);
                throw e;
            }
        }

        public void ClientConnect(Cookie cookie)
        {
            try
            {
                string url = _ci.getServiceUrl();
                if (url != null)
                {
                    _connection.Connect(url, cookie);
                }
                else
                {
                    throw new ArgumentHandlingException("Missing Arguement: url");
                }
            }
            catch (Exception e)
            {
                _ci.getUtil().LogException(e);
                throw e;
            }
        }

        // Save The Session
        public void ClientSaveSession(String fileName)
        {
            String urlString = GetUrlString();
            _connection.SaveSession(fileName, urlString);
        }

        public void ClientLoadSession()
        {
            String saveSessionFileName = _ci.get_option("sessionfile");
            _connection.LoadSession(saveSessionFileName, _ci.getServiceUrl());
        }

        private String GetUrlString()
        {
            if (_ci.getServiceUrl() != null)
            {
                return _ci.getServiceUrl();
            }
            else
            {
                return "https://" + _ci.get_option("server") + "/sdk/vimService";
            }
        }

        /// <summary>
        /// Disconnect from the service
        /// </summary>
        public void ClientDisconnect()
        {
            try
            {
                _connection.Disconnect();
            }
            catch (Exception e)
            {
                //_ci.Util.LogException(e);
                throw e;
            }
        }
        static String[] meTree = {
      "ManagedEntity",
      "ComputeResource",
      "ClusterComputeResource",
      "Datacenter",
      "Folder",
      "HostSystem",
      "ResourcePool",
      "VirtualMachine"
      };
        static String[] crTree = {
      "ComputeResource",
      "ClusterComputeResource"
      };
        static String[] hcTree = {
      "HistoryCollector",
      "EventHistoryCollector",
      "TaskHistoryCollector"
   };

        public Boolean typeIsA(String searchType, String foundType)
        {
            if (searchType.Equals(foundType))
            {
                return true;
            }
            else if (searchType.Equals("ManagedEntity"))
            {
                for (int i = 0; i < meTree.Length; ++i)
                {
                    if (meTree[i].Equals(foundType))
                    {
                        return true;
                    }
                }
            }
            else if (searchType.Equals("ComputeResource"))
            {
                for (int i = 0; i < crTree.Length; ++i)
                {
                    if (crTree[i].Equals(foundType))
                    {
                        return true;
                    }
                }
            }
            else if (searchType.Equals("HistoryCollector"))
            {
                for (int i = 0; i < hcTree.Length; ++i)
                {
                    if (hcTree[i].Equals(foundType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Get an entity of specified type with the name specified
        /// If name is null, will return the 1st matching entity of the type
        /// </summary>
        /// <param name="root">a root folder if available, or null for default</param>
        /// <param name="type">the type of the entity - e.g. VirtualMachine</param>
        /// <param name="name">name to match</param>
        /// <returns>
        ///    ManagedObjectReference of 1st type found, if name is null,
        ///    null if name not matched,
        ///    a ManagedObjectReference if name not null and match found.
        /// </returns>
        public ManagedObjectReference GetDecendentMoRef(
           ManagedObjectReference root, string type, string name
        )
        {
            if (name == null || name.Length == 0)
            {
                return null;
            }

            string[][] typeinfo =
               new string[][] { new string[] { type,  "name", },
         };

            ObjectContent[] ocary =
               GetContentsRecursively(null, root, typeinfo, true);

            if (ocary == null || ocary.Length == 0)
            {
                return null;
            }

            ObjectContent oc = null;
            ManagedObjectReference mor = null;
            DynamicProperty[] propary = null;
            string propval = null;
            bool found = false;
            for (int oci = 0; oci < ocary.Length && !found; oci++)
            {
                oc = ocary[oci];
                mor = oc.obj;
                propary = oc.propSet;

                if ((type == null) || (type != null && typeIsA(type, mor.type)))
                {
                    if (propary.Length > 0)
                    {
                        propval = (string)propary[0].val;
                    }

                    found = propval != null && name.Equals(propval);
                    propval = null;
                }
            }

            if (!found)
            {
                mor = null;
            }

            return mor;
        }


        /// <summary>
        /// Get the First MOR from a root of the specified type
        /// </summary>
        /// <param name="root">a root folder if available, or null for default</param>
        /// <param name="type">the type of the entity - e.g. VirtualMachine</param>
        /// <returns>managed object reference available</returns>
        public ManagedObjectReference GetFirstDecendentMoRef(
           ManagedObjectReference root, string type
        )
        {
            ArrayList morlist = GetDecendentMoRefs(root, type);

            ManagedObjectReference mor = null;

            if (morlist.Count > 0)
            {
                mor = (ManagedObjectReference)((object[])morlist[0])[0];
            }
            else
            {
                throw new Exception("Unable to find " + type + " in Inventory");
            }

            return mor;
        }

        /// <summary>
        /// Retrieve all container refs of the type specified.
        /// </summary>
        /// <param name="root">a root folder if available, or null for default</param>
        /// <param name="type">type of container refs to retrieve</param>
        /// <returns>List of MORefs</returns>
        public ArrayList GetDecendentMoRefs(ManagedObjectReference root, string type)
        {
            string[][] typeinfo =
               new string[][] { new string[] { type,  "name", },
         };

            ObjectContent[] ocary =
               GetContentsRecursively(null, root, typeinfo, true);

            ArrayList refs = new ArrayList();
            if (ocary == null || ocary.Length == 0)
            {
                return refs;
            }

            ObjectContent oc = null;
            ManagedObjectReference mor = null;
            DynamicProperty[] propary = null;
            string propval = null;

            for (int oci = 0; oci < ocary.Length; oci++)
            {
                oc = ocary[oci];
                mor = oc.obj;
                propary = oc.propSet;

                if ((type == null) || (type != null && mor.type.Equals(type)))
                {
                    if (propary.Length > 0)
                    {
                        propval = (string)propary[0].val;
                    }

                    refs.Add(new object[] { mor, propval });
                    propval = null;
                }
            }

            return refs;
        }
        public ArrayList GetDecendentMoRefs(
             ManagedObjectReference root, String type, String[][] filter
          )
        {
            String[][] typeinfo = new String[][] { new String[] { type, "name" }, };
            ObjectContent[] ocary =
               GetContentsRecursively(null, root, typeinfo, true);
            ArrayList refs = new ArrayList();
            if (ocary == null || ocary.Length == 0)
            {
                return refs;
            }
            for (int oci = 0; oci < ocary.Length; oci++)
            {
                refs.Add(ocary[oci].obj);
            }
            if (filter != null)
            {
                ArrayList filtermors = filterMOR(refs, filter);
                return filtermors;
            }
            else
            {
                return refs;
            }
        }
        private ArrayList filterMOR(ArrayList mors, String[][] filter)
        {
            ArrayList filteredmors = new ArrayList();
            for (int i = 0; i < mors.Count; i++)
            {
                Boolean flag = true;

                for (int k = 0; k < filter.Length; k++)
                {
                    String prop = filter[k][0];
                    String reqVal = filter[k][1];
                    String value = getProp(((ManagedObjectReference)mors[i]), prop);
                    if (reqVal == null)
                    {
                        continue;

                    }
                    if (value == null && reqVal == null)
                    {
                        continue;
                    }
                    if (value == null && reqVal != null)
                    {
                        flag = false;
                        k = filter.Length + 1;
                    }
                    else if (value.Equals(reqVal))
                    {
                    }
                    else
                    {
                        flag = false;
                        k = filter.Length + 1;
                    }
                }
                if (flag)
                {
                    filteredmors.Add(mors[i]);
                }
            }
            return filteredmors;
        }

        public String getProp(ManagedObjectReference obj, String prop)
        {
            String propVal = null;
            try
            {
                propVal = (String)GetDynamicProperty(obj, prop);
            }
            catch (Exception e) { }
            return propVal;
        }

        /// <summary>
        /// Retrieve Container contents for all containers recursively from root
        /// </summary>
        /// <returns>retrieved object contents</returns>
        public ObjectContent[] GetAllContainerContents()
        {
            ObjectContent[] ocary =
               GetContentsRecursively(null, true);

            return ocary;
        }

        /// <summary>
        /// Retrieve container contents from specified root recursively if requested.
        /// </summary>
        /// <param name="root">a root folder if available, or null for default</param>
        /// <param name="recurse">retrieve contents recursively from the root down</param>
        /// <returns>retrieved object contents</returns>
        public ObjectContent[] GetContentsRecursively(
           ManagedObjectReference root, bool recurse
        )
        {
            string[][] typeinfo =
               new string[][] { new string[] { "ManagedEntity", },
         };

            ObjectContent[] ocary =
               GetContentsRecursively(null, root, typeinfo, recurse);

            return ocary;
        }

        /// <summary>
        /// convenience function to retrieve content recursively with multiple properties.
        /// the typeinfo array contains typename + properties to retrieve.
        /// </summary>
        /// <param name="collector">a property collector if available or null for default</param>
        /// <param name="root">a root folder if available, or null for default</param>
        /// <param name="typeinfo">2D array of properties for each typename</param>
        /// <param name="recurse">retrieve contents recursively from the root down</param>
        /// <returns>retrieved object contents</returns>
        public ObjectContent[] GetContentsRecursively(
           ManagedObjectReference collector, ManagedObjectReference root,
           string[][] typeinfo, bool recurse
        )
        {
            if (typeinfo == null || typeinfo.Length == 0)
            {
                return null;
            }

            ManagedObjectReference usecoll = collector;
            if (usecoll == null)
            {
                usecoll = _connection.PropCol;
            }

            ManagedObjectReference useroot = root;
            if (useroot == null)
            {
                useroot = _connection.Root;
            }
            SelectionSpec[] selectionSpecs = null;
            // Modified by Satyendra on 19th May
            if (recurse)
            {
                selectionSpecs = buildFullTraversal();
            }

            PropertySpec[] propspecary = BuildPropertySpecArray(typeinfo);

            PropertyFilterSpec spec = new PropertyFilterSpec();
            spec.propSet = propspecary;
            spec.objectSet = new ObjectSpec[] { new ObjectSpec() };
            spec.objectSet[0].obj = useroot;
            spec.objectSet[0].skip = false;
            spec.objectSet[0].selectSet = selectionSpecs;

            ObjectContent[] retoc =
               retrievePropertiesEx(usecoll, new PropertyFilterSpec[] { spec });

            return retoc;
        }

        /// <summary>
        /// Get a MORef from the property returned.
        /// </summary>
        /// <param name="objMor">Object to get a reference property from</param>
        /// <param name="propName">name of the property that is the MORef</param>
        /// <returns>the MORef for that property.</returns>
        public ManagedObjectReference GetMoRefProp(ManagedObjectReference objMor, string propName)
        {
            if (objMor == null)
            {
                throw new Exception("Need an Object Reference to get Contents from.");
            }

            // If no property specified, assume childEntity
            if (propName == null)
            {
                propName = "childEntity";
            }

            ObjectContent[] objcontent =
               GetObjectProperties(
                  null, objMor, new string[] { propName }
               );

            ManagedObjectReference propmor = null;
            if (objcontent.Length > 0 && objcontent[0].propSet.Length > 0)
            {
                propmor = (ManagedObjectReference)objcontent[0].propSet[0].val;
            }
            else
            {
                throw new Exception("Did not find first " + propName + " in " + objMor.type);
            }

            return propmor;
        }

        /// <summary>
        /// Retrieve contents for a single object based on the property collector
        /// registered with the service.
        /// </summary>
        /// <param name="collector">Property collector registered with service</param>
        /// <param name="mobj">Managed Object Reference to get contents for</param>
        /// <param name="properties">names of properties of object to retrieve</param>
        /// <returns>retrieved object contents</returns>
        public ObjectContent[] GetObjectProperties(
           ManagedObjectReference collector,
           ManagedObjectReference mobj, string[] properties
        )
        {
            if (mobj == null)
            {
                return null;
            }

            ManagedObjectReference usecoll = collector;
            if (usecoll == null)
            {
                usecoll = _connection.PropCol;
            }

            PropertyFilterSpec spec = new PropertyFilterSpec();
            spec.propSet = new PropertySpec[] { new PropertySpec() };
            spec.propSet[0].all = properties == null || properties.Length == 0;
            spec.propSet[0].allSpecified = spec.propSet[0].all;
            spec.propSet[0].type = mobj.type;
            spec.propSet[0].pathSet = properties;

            spec.objectSet = new ObjectSpec[] { new ObjectSpec() };
            spec.objectSet[0].obj = mobj;
            spec.objectSet[0].skip = false;

            return retrievePropertiesEx(usecoll, new PropertyFilterSpec[] { spec });
        }

        public Object GetDynamicProperty(ManagedObjectReference mor, String propertyName)
        {
            ObjectContent[] objContent = GetObjectProperties(null, mor,
                  new String[] { propertyName });

            Object propertyValue = null;
            if (objContent != null)
            {
                DynamicProperty[] dynamicProperty = objContent[0].propSet;
                if (dynamicProperty != null)
                {
                    Object dynamicPropertyVal = dynamicProperty[0].val;
                    String dynamicPropertyName = dynamicPropertyVal.GetType().FullName;
                    propertyValue = dynamicPropertyVal;

                }
            }
            return propertyValue;
        }

        public String WaitForTask(ManagedObjectReference taskmor)
        {
            Object[] result = WaitForValues(
                              taskmor, new String[] { "info.state", "info.error" },
                              new String[] { "state" },
                              new Object[][] { new Object[] { TaskInfoState.success, TaskInfoState.error } });
            if (result[0].Equals(TaskInfoState.success))
            {
                return "sucess";
            }
            else
            {
                TaskInfo tinfo = (TaskInfo)GetDynamicProperty(taskmor, "info");
                LocalizedMethodFault fault = tinfo.error;
                String error = "Error Occured";
                if (fault != null)
                {
                    error = fault.localizedMessage;
                    Console.WriteLine("Fault " + fault.fault.ToString());
                    Console.WriteLine("Message " + fault.localizedMessage);
                }
                return error;
            }
        }

        /// <summary>
        /// Handle Updates for a single object.
        /// waits till expected values of properties to check are reached
        /// Destroys the ObjectFilter when done.
        /// </summary>
        /// <param name="objmor">MOR of the Object to wait for</param>
        /// <param name="filterProps">Properties list to filter</param>
        /// <param name="endWaitProps">
        ///   Properties list to check for expected values
        ///   these be properties of a property in the filter properties list
        /// </param>
        /// <param name="expectedVals">values for properties to end the wait</param>
        /// <returns>true indicating expected values were met, and false otherwise</returns>
        public object[] WaitForValues(
           ManagedObjectReference objmor, string[] filterProps,
           string[] endWaitProps, object[][] expectedVals
        )
        {
            // version string is initially null
            string version = "";
            object[] endVals = new object[endWaitProps.Length];
            object[] filterVals = new object[filterProps.Length];

            PropertyFilterSpec spec = new PropertyFilterSpec();
            spec.objectSet = new ObjectSpec[] { new ObjectSpec() };
            spec.objectSet[0].obj = objmor;

            spec.propSet = new PropertySpec[] { new PropertySpec() };
            spec.propSet[0].pathSet = filterProps;
            spec.propSet[0].type = objmor.type;

            spec.objectSet[0].selectSet = null;
            spec.objectSet[0].skip = false;
            spec.objectSet[0].skipSpecified = true;

            ManagedObjectReference filterSpecRef =
               _connection.Service.CreateFilter(
                  _connection.PropCol, spec, true
               );

            bool reached = false;

            UpdateSet updateset = null;
            PropertyFilterUpdate[] filtupary = null;
            PropertyFilterUpdate filtup = null;
            ObjectUpdate[] objupary = null;
            ObjectUpdate objup = null;
            PropertyChange[] propchgary = null;
            PropertyChange propchg = null;
            while (!reached)
            {
                updateset =
                   _connection.Service.WaitForUpdates(
                      _connection.PropCol, version
                   );

                version = updateset.version;

                if (updateset == null || updateset.filterSet == null)
                {
                    continue;
                }

                // Make this code more general purpose when PropCol changes later.
                filtupary = updateset.filterSet;
                filtup = null;
                for (int fi = 0; fi < filtupary.Length; fi++)
                {
                    filtup = filtupary[fi];
                    objupary = filtup.objectSet;
                    objup = null;
                    propchgary = null;
                    for (int oi = 0; oi < objupary.Length; oi++)
                    {
                        objup = objupary[oi];

                        // TODO: Handle all "kind"s of updates.
                        if (objup.kind == ObjectUpdateKind.modify ||
                           objup.kind == ObjectUpdateKind.enter ||
                           objup.kind == ObjectUpdateKind.leave
                           )
                        {
                            propchgary = objup.changeSet;
                            for (int ci = 0; ci < propchgary.Length; ci++)
                            {
                                propchg = propchgary[ci];
                                UpdateValues(endWaitProps, endVals, propchg);
                                UpdateValues(filterProps, filterVals, propchg);
                            }
                        }
                    }
                }

                object expctdval = null;
                // Check if the expected values have been reached and exit the loop if done.
                // Also exit the WaitForUpdates loop if this is the case.
                for (int chgi = 0; chgi < endVals.Length && !reached; chgi++)
                {
                    for (int vali = 0; vali < expectedVals[chgi].Length && !reached; vali++)
                    {
                        expctdval = expectedVals[chgi][vali];
                        reached = expctdval.Equals(endVals[chgi]) || reached;
                    }
                }
            }
            // Destroy the filter when we are done.
            _connection.Service.DestroyPropertyFilter(filterSpecRef);
            return filterVals;
        }

        /// <summary>
        /// Utilty method to overcome the C# stubs limitation, of not generating all the ENUMS
        /// </summary>
        /// <param name="valueToCheck">string value to compare</param>
        /// <param name="rawXMLNode">Instance of the ENUM returned from the server in the form of
        /// XmlNode[] array containing the attribute type and the text value</param>
        /// <returns></returns>
        private string getValueOfUnrecognizedEnumInstance(XmlNode[] rawXMLNode)
        {
            return (from p in rawXMLNode
                        where p is XmlText
                        select p.Value).First();
        }

        /// <summary>
        /// set values into the return array
        /// </summary>
        /// <param name="props">property names</param>
        /// <param name="vals">return array</param>
        /// <param name="propchg">Change received</param>
        protected void UpdateValues(string[] props, object[] vals, PropertyChange propchg)
        {
            for (int findi = 0; findi < props.Length; findi++)
            {
                if (propchg.name.LastIndexOf(props[findi]) >= 0)
                {
                    if (propchg.op == PropertyChangeOp.remove)
                    {
                        vals[findi] = "";
                    }
                    else
                    {
                        var value = propchg.val;
                        if (propchg.val is XmlNode[]) // This happens only for the ENUMs whose definitions are not there.
                        {
                            value = getValueOfUnrecognizedEnumInstance(propchg.val as XmlNode[]);
                        }
                        vals[findi] = value;
                    }
                }
            }
        }
        /**
            * This method creates a SelectionSpec[] to traverses the entire
            * inventory tree starting at a Folder
            * @return The SelectionSpec[]
            */
        public SelectionSpec[] buildFullTraversal()
        {
            // Recurse through all ResourcePools

            TraversalSpec rpToVm = new TraversalSpec();
            rpToVm.name = "rpToVm";
            rpToVm.type = "ResourcePool";
            rpToVm.path = "vm";
            rpToVm.skip = false;
            rpToVm.skipSpecified = true;

            // Recurse through all ResourcePools

            TraversalSpec rpToRp = new TraversalSpec();
            rpToRp.name = "rpToRp";
            rpToRp.type = "ResourcePool";
            rpToRp.path = "resourcePool";
            rpToRp.skip = false;
            rpToRp.skipSpecified = true;

            rpToRp.selectSet = new SelectionSpec[] { new SelectionSpec(), new SelectionSpec() };
            rpToRp.selectSet[0].name = "rpToRp";
            rpToRp.selectSet[1].name = "rpToVm";


            // Traversal through ResourcePool branch
            TraversalSpec crToRp = new TraversalSpec();
            crToRp.name = "crToRp";
            crToRp.type = "ComputeResource";
            crToRp.path = "resourcePool";
            crToRp.skip = false;
            crToRp.skipSpecified = true;
            crToRp.selectSet = new SelectionSpec[] { new SelectionSpec(), new SelectionSpec() };
            crToRp.selectSet[0].name = "rpToRp";
            crToRp.selectSet[1].name = "rpToVm";


            // Traversal through host branch
            TraversalSpec crToH = new TraversalSpec();
            crToH.name = "crToH";
            crToH.type = "ComputeResource";
            crToH.path = "host";
            crToH.skip = false;
            crToH.skipSpecified = true;

            // Traversal through hostFolder branch
            TraversalSpec dcToHf = new TraversalSpec();
            dcToHf.name = "dcToHf";
            dcToHf.type = "Datacenter";
            dcToHf.path = "hostFolder";
            dcToHf.skip = false;
            dcToHf.selectSet = new SelectionSpec[] { new SelectionSpec() };
            dcToHf.selectSet[0].name = "visitFolders";


            // Traversal through vmFolder branch
            TraversalSpec dcToVmf = new TraversalSpec();
            dcToVmf.name = "dcToVmf";
            dcToVmf.type = "Datacenter";
            dcToVmf.path = "vmFolder";
            dcToVmf.skip = false;
            dcToVmf.skipSpecified = true;
            dcToVmf.selectSet = new SelectionSpec[] { new SelectionSpec() };
            dcToVmf.selectSet[0].name = "visitFolders";


            // Recurse through all Hosts
            TraversalSpec HToVm = new TraversalSpec();
            HToVm.name = "HToVm";
            HToVm.type = "HostSystem";
            HToVm.path = "vm";
            HToVm.skip = false;
            HToVm.skipSpecified = true;
            HToVm.selectSet = new SelectionSpec[] { new SelectionSpec() };
            HToVm.selectSet[0].name = "visitFolders";

            //Traversal spec from Dataceneter to Network

            TraversalSpec dctonw = new TraversalSpec();
            dctonw.name = "dctonw";
            dctonw.type = "Datacenter";
            dctonw.path = "networkFolder";
            dctonw.skip = false;
            dctonw.skipSpecified = true;
            dctonw.selectSet = new SelectionSpec[] { new SelectionSpec() };
            dctonw.selectSet[0].name = "visitFolders";

            // Recurse thriugh the folders
            TraversalSpec visitFolders = new TraversalSpec();
            visitFolders.name = "visitFolders";
            visitFolders.type = "Folder";
            visitFolders.path = "childEntity";
            visitFolders.skip = false;
            visitFolders.skipSpecified = true;
            visitFolders.selectSet = new SelectionSpec[] { new SelectionSpec(), new SelectionSpec(), new SelectionSpec(), new SelectionSpec(), new SelectionSpec(), new SelectionSpec(), new SelectionSpec(), new SelectionSpec() };
            visitFolders.selectSet[0].name = "visitFolders";
            visitFolders.selectSet[1].name = "dcToHf";
            visitFolders.selectSet[2].name = "dcToVmf";
            visitFolders.selectSet[3].name = "crToH";
            visitFolders.selectSet[4].name = "crToRp";
            visitFolders.selectSet[5].name = "HToVm";
            visitFolders.selectSet[6].name = "rpToVm";
            visitFolders.selectSet[7].name = "dctonw";
            return new SelectionSpec[] { visitFolders, dcToVmf, dctonw, dcToHf, crToH, crToRp, rpToRp, HToVm, rpToVm };


        }
        /// <summary>
        /// This code takes an array of [typename, property, property, ...]
        /// and converts it into a ContainerFilterSpec array.
        /// handles case where multiple references to the same typename
        /// are specified.
        /// </summary>
        /// <param name="typeinfo">array pf [typename, property, property, ...]</param>
        /// <returns>array of container property specs</returns>
        public PropertySpec[] BuildPropertySpecArray(
           string[][] typeinfo
        )
        {
            // Eliminate duplicates
            Hashtable tInfo = new Hashtable();
            for (int ti = 0; ti < typeinfo.Length; ++ti)
            {
                Hashtable props = (Hashtable)tInfo[typeinfo[ti][0]];
                if (props == null)
                {
                    props = new Hashtable();
                    tInfo[typeinfo[ti][0]] = props;
                }
                bool typeSkipped = false;
                for (int pi = 0; pi < typeinfo[ti].Length; ++pi)
                {
                    String prop = typeinfo[ti][pi];
                    if (typeSkipped)
                    {
                        if (!props.Contains(prop))
                        {
                            // some value, not important
                            props[prop] = String.Empty;
                        }
                    }
                    else
                    {
                        typeSkipped = true;
                    }
                }
            }

            // Create PropertySpecs
            ArrayList pSpecs = new ArrayList();
            foreach (String type in tInfo.Keys)
            {
                PropertySpec pSpec = new PropertySpec();
                Hashtable props = (Hashtable)tInfo[type];
                pSpec.type = type;
                pSpec.all = props.Count == 0 ? true : false;
                pSpec.pathSet = new String[props.Count];
                int index = 0;
                foreach (String prop in props.Keys)
                {
                    pSpec.pathSet[index++] = prop;
                }
                pSpecs.Add(pSpec);
            }

            return (PropertySpec[])pSpecs.ToArray(typeof(PropertySpec));
        }
        public SelectionSpec[] GetStorageTraversalSpec()
        {
            SelectionSpec ssFolders = new SelectionSpec();
            ssFolders.name = "visitFolders";

            TraversalSpec datacenterSpec = new TraversalSpec();
            datacenterSpec.name = "dcTodf";
            datacenterSpec.type = "Datacenter";
            datacenterSpec.path = "datastoreFolder";
            datacenterSpec.skip = false;
            datacenterSpec.selectSet = new SelectionSpec[] { ssFolders };

            TraversalSpec visitFolder = new TraversalSpec();
            visitFolder.type = "Folder";
            visitFolder.name = "visitFolders";
            visitFolder.path = "childEntity";
            visitFolder.skip = false;

            SelectionSpec[] ssSpecList = new SelectionSpec[] { new SelectionSpec(), new SelectionSpec() };
            ssSpecList[0] = datacenterSpec;
            ssSpecList[1] = ssFolders;

            visitFolder.selectSet = ssSpecList;
            return (new SelectionSpec[] { visitFolder });
        }



        # region Updated Inventory Query Methods

        public ObjectContent[] retrievePropertiesEx(ManagedObjectReference propertyCollector, PropertyFilterSpec[] specs)
        {
            return retrievePropertiesEx(propertyCollector, specs, null);
        }

        public ObjectContent[] retrievePropertiesEx(ManagedObjectReference propertyCollector, PropertyFilterSpec[] specs, int? maxObjects)
        {
            List<ObjectContent> objectList = new List<ObjectContent>();
            var result =
                     _connection.Service.RetrievePropertiesEx(propertyCollector, specs, new RetrieveOptions()
                     {
                         maxObjects = maxObjects.GetValueOrDefault(),
                         maxObjectsSpecified = (maxObjects != null)
                     });
            if (result != null)
            {
                string token = result.token;
                objectList.AddRange(result.objects.AsEnumerable());
                while (token != null && !string.Empty.Equals(token))
                {
                    result = _connection.Service.ContinueRetrievePropertiesEx(propertyCollector, token);
                    if (result != null)
                    {
                        token = result.token;
                        objectList.AddRange(result.objects.AsEnumerable());
                    }
                }
            }
            return objectList.ToArray();
        }

        private ObjectContent[] queryContainerView(ManagedObjectReference containerRoot, string morefType, string[] morefProperies)
        {
            List<ObjectContent> objectList = new List<ObjectContent>();
            var containerView = _connection.Service.CreateContainerView(_connection.ServiceContent.viewManager, containerRoot, new string[] { morefType }, true);
            PropertyFilterSpec spec = new PropertyFilterSpec();
            spec.propSet = new PropertySpec[] { new PropertySpec() };
            spec.propSet[0].all = morefProperies == null || morefProperies.Length == 0;
            spec.propSet[0].allSpecified = spec.propSet[0].all;
            spec.propSet[0].type = morefType;
            spec.propSet[0].pathSet = morefProperies;
            spec.objectSet = new ObjectSpec[] { new ObjectSpec() };
            TraversalSpec ts = new TraversalSpec();
            ts.name = "view";
            ts.path = "view";
            ts.skip = false;
            ts.type = "ContainerView";
            spec.objectSet[0].obj = containerView;
            spec.objectSet[0].selectSet = new SelectionSpec[] { ts };
            return retrievePropertiesEx(_connection.PropCol, new PropertyFilterSpec[] { spec }, null);
        }

        /// <summary>
        /// Returns map of entity of the given type and the name/value map of the properties queried inside the given container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="morefType"></param>
        /// <param name="morefProperies"></param>
        /// <returns></returns>
        public Dictionary<ManagedObjectReference, Dictionary<string, object>> getEntitiesInContainerByType(ManagedObjectReference container, string morefType, string[] morefProperies)
        {
            Dictionary<ManagedObjectReference, Dictionary<string, object>> returnVal = new Dictionary<ManagedObjectReference, Dictionary<string, object>>();
            ObjectContent[] contents = queryContainerView(container, morefType, morefProperies);
            foreach (ObjectContent content in contents)
            {
                Dictionary<string, object> propMap = new Dictionary<string, object>();
                foreach (DynamicProperty prop in content.propSet)
                {
                    propMap.Add(prop.name, prop.val);
                }
                returnVal.Add(content.obj, propMap);
            }

            return returnVal;
        }

        /// <summary>
        /// Returns map of entity of the given type and the name/value map of the properties queried from the entire inventory
        /// </summary>
        /// <param name="morefType"></param>
        /// <param name="morefProperies"></param>
        /// <returns></returns>
        public Dictionary<ManagedObjectReference, Dictionary<string, object>> getEntitiesByType(string morefType, string[] morefProperies)
        {
            return getEntitiesInContainerByType(_connection.Root, morefType, morefProperies);
        }

        /// <summary>
        /// Returns map of name and entity of the given type from the entire inventory
        /// </summary>
        /// <param name="morefType"></param>
        /// <returns></returns>
        public Dictionary<string, ManagedObjectReference> getNamedEntitiesByType(string morefType)
        {
            return getNamedEntitiesInContainerByType(_connection.Root, morefType);
        }

        /// <summary>
        /// Returns map of name and entity of the given type inside the given container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="morefType"></param>
        /// <returns></returns>
        public Dictionary<string, ManagedObjectReference> getNamedEntitiesInContainerByType(ManagedObjectReference container, string morefType)
        {
            Dictionary<string, ManagedObjectReference> returnVal = new Dictionary<string, ManagedObjectReference>();
            ObjectContent[] contents = queryContainerView(container, morefType, new string[] { "name" });
            foreach (ObjectContent content in contents)
            {
                returnVal.Add((string)content.propSet[0].val, content.obj);
            }
            return returnVal;
        }

        /// <summary>
        /// Searches the entire inventory for the entity with the given name and type
        /// </summary>
        /// <param name="morefType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ManagedObjectReference getEntityByName(string morefType, string name)
        {
            return getEntityInContainerByName(_connection.Root, morefType, name);
        }

        /// <summary>
        /// Searches the given container for the entity with the given name and type
        /// </summary>
        /// <param name="container"></param>
        /// <param name="morefType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ManagedObjectReference getEntityInContainerByName(ManagedObjectReference container, string morefType, string name)
        {
            ObjectContent[] contents = queryContainerView(container, morefType, new string[] { "name" });
            foreach (ObjectContent content in contents)
            {

                if (((string)content.propSet[0].val).Equals(name))
                {
                    return content.obj;
                }
            }
            return null;
        }

        #endregion
    }
}
