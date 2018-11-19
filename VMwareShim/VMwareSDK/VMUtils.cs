using System;
using Vim25Api;
using System.Collections.Generic;

namespace AppUtil
{
    public class VMUtils
    {
        private AppUtil cb;
        private SvcConnection _connection;

        public VMUtils(AppUtil argCB)
        {
            cb = argCB;
        }

        public void Init(AppUtil ci)
        {
            cb = ci;
            _connection = ci.getConnection();

        }
        public VirtualMachineConfigSpec createVmConfigSpec(String vmName,
                                                        String datastoreName,
                                                        int diskSizeMB,
                                                        ManagedObjectReference computeResMor,
                                                        ManagedObjectReference hostMor)
        {

            ConfigTarget configTarget = getConfigTargetForHost(computeResMor, hostMor);
            VirtualDevice[] defaultDevices = getDefaultDevices(computeResMor, hostMor);
            VirtualMachineConfigSpec configSpec = new VirtualMachineConfigSpec();

            String networkName = null;
            if (configTarget.network != null)
            {
                for (int i = 0; i < configTarget.network.Length; i++)
                {
                    VirtualMachineNetworkInfo netInfo = configTarget.network[i];
                    NetworkSummary netSummary = netInfo.network;
                    if (netSummary.accessible)
                    {
                        networkName = netSummary.name;
                        break;
                    }
                }
            }
            ManagedObjectReference datastoreRef = null;
            if (datastoreName != null)
            {
                Boolean flag = false;
                for (int i = 0; i < configTarget.datastore.Length; i++)
                {
                    VirtualMachineDatastoreInfo vdsInfo = configTarget.datastore[i];
                    DatastoreSummary dsSummary = vdsInfo.datastore;
                    if (dsSummary.name.Equals(datastoreName))
                    {
                        flag = true;
                        if (dsSummary.accessible)
                        {
                            datastoreName = dsSummary.name;
                            datastoreRef = dsSummary.datastore;
                        }
                        else
                        {
                            throw new Exception("Specified Datastore is not accessible");
                        }
                        break;
                    }
                }
                if (!flag)
                {
                    throw new Exception("Specified Datastore is not Found");
                }
            }
            else
            {
                Boolean flag = false;
                for (int i = 0; i < configTarget.datastore.Length; i++)
                {
                    VirtualMachineDatastoreInfo vdsInfo = configTarget.datastore[i];
                    DatastoreSummary dsSummary = vdsInfo.datastore;
                    if (dsSummary.accessible)
                    {
                        datastoreName = dsSummary.name;
                        datastoreRef = dsSummary.datastore;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    throw new Exception("No Datastore found on host");
                }
            }
            String datastoreVolume = getVolumeName(datastoreName);
            VirtualMachineFileInfo vmfi = new VirtualMachineFileInfo();
            vmfi.vmPathName = datastoreVolume;
            configSpec.files = vmfi;
            // Add a scsi controller
            int diskCtlrKey = 1;
            VirtualDeviceConfigSpec scsiCtrlSpec = new VirtualDeviceConfigSpec();
            scsiCtrlSpec.operation = VirtualDeviceConfigSpecOperation.add;
            scsiCtrlSpec.operationSpecified = true;
            VirtualLsiLogicController scsiCtrl = new VirtualLsiLogicController();
            scsiCtrl.busNumber = 0;
            scsiCtrlSpec.device = scsiCtrl;
            scsiCtrl.key = diskCtlrKey;
            scsiCtrl.sharedBus = VirtualSCSISharing.noSharing;
            String ctlrType = scsiCtrl.GetType().Name;


            // Find the IDE controller
            VirtualDevice ideCtlr = null;
            for (int di = 0; di < defaultDevices.Length; di++)
            {
                if (defaultDevices[di].GetType().Name.Equals("VirtualIDEController"))
                {
                    ideCtlr = defaultDevices[di];
                    break;
                }
            }

            // Add a floppy
            VirtualDeviceConfigSpec floppySpec = new VirtualDeviceConfigSpec();
            floppySpec.operation = VirtualDeviceConfigSpecOperation.add;
            floppySpec.operationSpecified = true;
            VirtualFloppy floppy = new VirtualFloppy();
            VirtualFloppyDeviceBackingInfo flpBacking = new VirtualFloppyDeviceBackingInfo();
            flpBacking.deviceName = "/dev/fd0";
            floppy.backing = flpBacking;
            floppy.key = 3;
            floppySpec.device = floppy;

            // Add a cdrom based on a physical device
            VirtualDeviceConfigSpec cdSpec = null;

            if (ideCtlr != null)
            {
                cdSpec = new VirtualDeviceConfigSpec();
                cdSpec.operation = VirtualDeviceConfigSpecOperation.add;
                cdSpec.operationSpecified = true;
                VirtualCdrom cdrom = new VirtualCdrom();
                VirtualCdromIsoBackingInfo cdDeviceBacking = new VirtualCdromIsoBackingInfo();
                cdDeviceBacking.datastore = datastoreRef;
                cdDeviceBacking.fileName = datastoreVolume + "testcd.iso";
                cdrom.backing = cdDeviceBacking;
                cdrom.key = 20;
                cdrom.controllerKey = ideCtlr.key;
                cdrom.controllerKeySpecified = true;
                cdrom.unitNumberSpecified = true;
                cdrom.unitNumber = 0;
                cdSpec.device = cdrom;
            }

            // Create a new disk - file based - for the vm
            VirtualDeviceConfigSpec diskSpec = null;
            diskSpec = createVirtualDisk(datastoreName, diskCtlrKey, datastoreRef, diskSizeMB);

            // Add a NIC. the network Name must be set as the device name to create the NIC.
            VirtualDeviceConfigSpec nicSpec = new VirtualDeviceConfigSpec();
            if (networkName != null)
            {
                nicSpec.operation = VirtualDeviceConfigSpecOperation.add;
                nicSpec.operationSpecified = true;
                VirtualEthernetCard nic = new VirtualPCNet32();
                VirtualEthernetCardNetworkBackingInfo nicBacking = new VirtualEthernetCardNetworkBackingInfo();
                nicBacking.deviceName = networkName;
                nic.addressType = "generated";
                nic.backing = nicBacking;
                nic.key = 4;
                nicSpec.device = nic;
            }

            var deviceConfigSpec = new List<VirtualDeviceConfigSpec>();
            deviceConfigSpec.Add(scsiCtrlSpec);
            deviceConfigSpec.Add(floppySpec);
            deviceConfigSpec.Add(diskSpec);
            deviceConfigSpec.Add(nicSpec);

            if (ideCtlr != null)
            {
                deviceConfigSpec.Add(cdSpec);
            }

            configSpec.deviceChange = deviceConfigSpec.ToArray();
            return configSpec;
        }
        /**
    * This method returns the ConfigTarget for a HostSystem
    * @param computeResMor A MoRef to the ComputeResource used by the HostSystem
    * @param hostMor A MoRef to the HostSystem
    * @return Instance of ConfigTarget for the supplied 
    * HostSystem/ComputeResource
    * @throws Exception When no ConfigTarget can be found
    */
        public ConfigTarget getConfigTargetForHost(ManagedObjectReference computeResMor,
                                                                    ManagedObjectReference hostMor)
        {

            ManagedObjectReference envBrowseMor =
               cb.getServiceUtil().GetMoRefProp(computeResMor, "environmentBrowser");

            ConfigTarget configTarget =
             cb.getConnection().Service.QueryConfigTarget(envBrowseMor, hostMor);

            if (configTarget == null)
            {
                throw new Exception("No ConfigTarget found in ComputeResource");
            }

            return configTarget;
        }
        /** 
         * The method returns the default devices from the HostSystem
         * @param computeResMor A MoRef to the ComputeResource used by the HostSystem
         * @param hostMor A MoRef to the HostSystem
         * @return Array of VirtualDevice containing the default devices for 
         * the HostSystem
         * @throws Exception
         */
        public VirtualDevice[] getDefaultDevices(ManagedObjectReference computeResMor,
                                                 ManagedObjectReference hostMor)
        {
            ManagedObjectReference envBrowseMor =
               cb.getServiceUtil().GetMoRefProp(computeResMor, "environmentBrowser");

            VirtualMachineConfigOption cfgOpt =
               cb.getConnection().Service.QueryConfigOption(envBrowseMor, null, hostMor);

            VirtualDevice[] defaultDevs = null;

            if (cfgOpt == null)
            {
                throw new Exception("No VirtualHardwareInfo found in ComputeResource");
            }
            else
            {
                defaultDevs = cfgOpt.defaultDevice;
                if (defaultDevs == null)
                {
                    throw new Exception("No Datastore found in ComputeResource");
                }
            }

            return defaultDevs;
        }
        private String getVolumeName(String volName)
        {
            String volumeName = null;
            if (volName != null && volName.Length > 0)
            {
                volumeName = "[" + volName + "]";
            }
            else
            {
                volumeName = "[Local]";
            }

            return volumeName;
        }
        /**
     * This method returns the contents of the hostFolder property from the
     * supplied Datacenter MoRef
     * @param dcmor MoRef to the Datacenter
     * @return MoRef to a Folder returned by the hostFolder property or
     * null if dcmor is NOT a MoRef to a Datacenter or if the hostFolder 
     * doesn't exist
     * @throws Exception
     */
        public ManagedObjectReference getHostFolder(ManagedObjectReference dcmor)
        {
            ManagedObjectReference hfmor =
               cb.getServiceUtil().GetMoRefProp(dcmor, "hostFolder");
            return hfmor;
        }
        /**
       * This method returns a MoRef to the HostSystem with the supplied name 
       * under the supplied Folder. If hostname is null, it returns the first
       * HostSystem found under the supplied Folder
       * @param hostFolderMor MoRef to the Folder to look in
       * @param hostname Name of the HostSystem you are looking for
       * @return MoRef to the HostSystem or null if not found
       * @throws Exception
       */
        public ManagedObjectReference getHost(ManagedObjectReference hostFolderMor, String hostname)
        {
            ManagedObjectReference hostmor = null;

            if (hostname != null)
            {
                hostmor =
                   cb.getServiceUtil().GetDecendentMoRef(hostFolderMor, "HostSystem", hostname);
            }
            else
            {
                hostmor = cb.getServiceUtil().GetFirstDecendentMoRef(hostFolderMor, "HostSystem");
            }

            return hostmor;
        }

        public VirtualDeviceConfigSpec createVirtualDisk(String volName,
                                                    int diskCtlrKey,
                                                    ManagedObjectReference datastoreRef,
                                                    int diskSizeMB)
        {
            String volumeName = getVolumeName(volName);
            VirtualDeviceConfigSpec diskSpec = new VirtualDeviceConfigSpec();

            diskSpec.fileOperation = VirtualDeviceConfigSpecFileOperation.create;
            diskSpec.fileOperationSpecified = true;
            diskSpec.operation = VirtualDeviceConfigSpecOperation.add;
            diskSpec.operationSpecified = true;

            VirtualDisk disk = new VirtualDisk();
            VirtualDiskFlatVer2BackingInfo diskfileBacking = new VirtualDiskFlatVer2BackingInfo();

            diskfileBacking.fileName = volumeName;
            diskfileBacking.diskMode = "persistent";

            disk.key = 0;
            disk.controllerKey = diskCtlrKey;
            disk.unitNumber = 0;
            disk.backing = diskfileBacking;
            disk.capacityInKB = diskSizeMB;
            disk.controllerKeySpecified = true;
            disk.unitNumberSpecified = true;


            diskSpec.device = disk;

            return diskSpec;
        }
    }

}
