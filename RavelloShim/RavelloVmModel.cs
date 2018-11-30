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
using Newtonsoft.Json;

namespace RavelloShim
{
	// Everything in here is used only to deserialize VM data from the Ravello API

	public enum LoadingStatus { UPLODAING, PARSING, PENDING, PAUSED, SAVING, DONE, DELETING, DELETED, ERROR_UPLOADING, ERROR, UNKNOWN, POST_SNAPSHOTTING, PRE_SNAPSHOTTING, SNAPSHOTTING };
	public enum SizeUnit { GB, MB, KB, BYTE };

	[JsonObject]
	public class RavelloVmList
    {
        public List<RavelloVmContainer> ravelloVms;
    }

	[JsonObject]
	public class RavelloVmContainer
    {
        public RavelloVm ravelloVm;
    }

	[JsonObject]
	public class RavelloBlueprint
	{
		public long id;
		public string description;
		public string name;
		public bool published;
		public RavelloDesign design;
	}

	[JsonObject]
	public class RavelloDesign
	{
		public List<RavelloVm> vms;
	}

	[JsonObject]
	public class RavelloVm
    {
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string os { get; set; }
        public string state { get; set; }
		public Size memorySize;
		public int numCpus;
		public bool supportsCloudInit;
		public long applicationId;
		public LoadingStatus loadingStatus;
		public int loadingPercentage;
		public long baseVmId;
		public List<HardDrive> hardDrives = new List<HardDrive>();
		public int vmOrderGroupId;
		public long stopTimeOut;
		public bool powerOffOnStopTimeOut;

    }

	[JsonObject]
	public class HardDrive
	{
		public bool boot;
		public string controller;
		public int controllerIndex;
		public int controllerPciSlot;
		public int index;
		public string name;
		public long baseDiskImageId;
		public string baseDiskImageName;
		internal string imageFetchMode;
	}

	[JsonObject]
	public class Owner
	{
		public long ownerId;
		public string name;
		public bool deleted;
	}

	[JsonObject]
	public class Size
	{
		public long value;
		public SizeUnit unit;
	}

	[JsonObject]
	public class DiskImage
	{
		public long id;
		public string name;
		public long creationTime;
		public LoadingStatus loadingStatus;
		public int loadingPercentage;
		public string owner;
		public Owner ownerDetails;
		public Size size;
		public bool isPublic;
		public long peerToPeerShares;
		public long communityShares;
		public long copies;
		public bool hasDocumentation;

	}

	[JsonObject]
	public class PublishApplicationPerformance
	{
		//public bool startAllVms = true;
		public string optimizationLevel = "PERFORMANCE_OPTIMIZED";// or PERFORMANCE_OPTIMIZED
		public string preferredRegion = "us-central-1";
	}

	[JsonObject]
	public class PublishApplicationCost
	{
		//public bool startAllVms = true;
		public string optimizationLevel = "COST_OPTIMIZED";// or PERFORMANCE_OPTIMIZED
														   //public string preferredRegion = "us-central-1";
	}

	[JsonObject]
	public class Application
	{
		public long id;
		public string name;
		public string description;
		public long baseBlueprintId;
		public Design design;
		//public List<VM> vms;
		//public Network network;
		//public NetworkFilter networkFilter;
		//public VMOrderGroups vmOrderGroups;
	}

	[JsonObject]
	public class Design
	{
		//public State state;
		public List<VMOrderGroup> vmOrderGroups = new List<VMOrderGroup>();
		public List<RavelloVm> vms = new List<RavelloVm>();
		//public Network network;
		//public NetworkFilter networkFilter;
		//public string validationMessages;
		public bool stopVmsByOrder = false;
	}

	[JsonObject]
	public class VMOrderGroup
	{
		public long id;
		public string name;
		public long order;
		public int delay;
	}
}
