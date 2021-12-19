using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.External
{
    /**
     * Add 4 different channel based on the resources from CommunityResourcePack.
     * 2 of those channels are specific to TAC Life Support, 2 of those are custom
     * channels where the type of resources used is defined in the config file.
     * */
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class CRPWrapper : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct TACLSResourceStruct
        {
            public float CurrentFood;
            public float MaxFood;
            public float CurrentWater;
            public float MaxWater;
            public float CurrentOxygen;
            public float MaxOxygen;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct TACLSWasteStruct
        {
            public float CurrentWaste;
            public float MaxWaste;
            public float CurrentLiquidWaste;
            public float MaxLiquidWaste;
            public float CurrentCO2;
            public float MaxCO2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct CustomResourceStruct
        {
            public float CurrentResource1;
            public float MaxResource1;
            public float CurrentResource2;
            public float MaxResource2;
            public float CurrentResource3;
            public float MaxResource3;
            public float CurrentResource4;
            public float MaxResource4;
        }

        private TACLSResourceStruct resourceStruct;
        private TACLSWasteStruct wasteStruct;
        private CustomResourceStruct customResource1, customResource2;

        private EventData<byte, object> resourceChannel, wasteChannel, custom1Channel, custom2Channel;

        // IDs of resources we care about
        private int? FoodID, WaterID, OxygenID, WasteID, LiquidWasteID, CO2ID,
            custom11ID, custom12ID, custom13ID, custom14ID,
            custom21ID, custom22ID, custom23ID, custom24ID;

        private bool TACLSFound, custom1Found, custom2Found;

        public void Start()
        {
            ARPWrapper.InitKSPARPWrapper();

            if (ARPWrapper.APIReady)
            {
                Config.KerbalSimpitConfig config = new Config.KerbalSimpitConfig();

                if (config.CustomResourceMessages.Count >= 2)
                {
                    custom11ID = GetResourceID(config.CustomResourceMessages[0].resourceName1);
                    custom12ID = GetResourceID(config.CustomResourceMessages[0].resourceName2);
                    custom13ID = GetResourceID(config.CustomResourceMessages[0].resourceName3);
                    custom14ID = GetResourceID(config.CustomResourceMessages[0].resourceName4);

                    custom21ID = GetResourceID(config.CustomResourceMessages[1].resourceName1);
                    custom22ID = GetResourceID(config.CustomResourceMessages[1].resourceName2);
                    custom23ID = GetResourceID(config.CustomResourceMessages[1].resourceName3);
                    custom24ID = GetResourceID(config.CustomResourceMessages[1].resourceName4);
                }
                else
                {
                    custom11ID = custom12ID = custom13ID = custom14ID = null;
                    custom21ID = custom22ID = custom23ID = custom24ID = null;
                }

                FoodID = GetResourceID("Food");
                WaterID = GetResourceID("Water");
                OxygenID = GetResourceID("Oxygen");
                WasteID = GetResourceID("Waste");
                LiquidWasteID = GetResourceID("WasteWater");
                CO2ID = GetResourceID("CarbonDioxide");

                if(FoodID != null && WaterID != null && OxygenID != null && WasteID != null && LiquidWasteID != null && CO2ID != null)
                {
                    KSPit.AddToDeviceHandler(ResourceProvider);
                    resourceChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TACLSResource);
                    KSPit.AddToDeviceHandler(WasteProvider);
                    wasteChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TACLSWaste);

                    TACLSFound = true; 
                    Debug.Log("KerbalSimpit: TACLS resources found. You can subscribe to the TACLS channels.");
                }
                else
                {
                    Debug.Log("KerbalSimpit: TACLS resources not found. I assume TACLS is not installed.");
                    TACLSFound = false;
                }

                if(custom11ID != null || custom12ID != null || custom13ID != null || custom14ID != null)
                {
                    KSPit.AddToDeviceHandler(Custom1Provider);
                    custom1Channel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.CustomResource1);
                    custom1Found = true;
                    Debug.Log("KerbalSimpit: Custom1 resources available.");
                }
                else
                {
                    Debug.Log("KerbalSimpit: Custom1 resources not available as no resources were found.");
                    custom1Found = false;
                }

                if (custom21ID != null || custom22ID != null || custom23ID != null || custom24ID != null)
                {
                    KSPit.AddToDeviceHandler(Custom2Provider);
                    custom2Channel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.CustomResource2);
                    custom2Found = true;
                    Debug.Log("KerbalSimpit: Custom2 resources available.");
                }
                else
                {
                    Debug.Log("KerbalSimpit: Custom2 resources not available as no resources were found.");
                    custom2Found = false;
                }

            } else
            {
                TACLSFound = false;
            }
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(ResourceProvider);
            KSPit.RemoveToDeviceHandler(WasteProvider);
            KSPit.RemoveToDeviceHandler(Custom1Provider);
            KSPit.RemoveToDeviceHandler(Custom2Provider);
        }

        public void Update()
        {
            if (TACLSFound)
            {
                getResourceValue(FoodID, ref resourceStruct.CurrentFood, ref resourceStruct.MaxFood);
                getResourceValue(WaterID, ref resourceStruct.CurrentWater, ref resourceStruct.MaxWater);
                getResourceValue(OxygenID, ref resourceStruct.CurrentOxygen, ref resourceStruct.MaxOxygen);

                getResourceValue(WasteID, ref wasteStruct.CurrentWaste, ref wasteStruct.MaxWaste);
                getResourceValue(LiquidWasteID, ref wasteStruct.CurrentLiquidWaste, ref wasteStruct.MaxLiquidWaste);
                getResourceValue(CO2ID, ref wasteStruct.CurrentCO2, ref wasteStruct.MaxCO2);
            }

            if (custom1Found)
            {
                getResourceValue(custom11ID, ref customResource1.CurrentResource1, ref customResource1.MaxResource1);
                getResourceValue(custom12ID, ref customResource1.CurrentResource2, ref customResource1.MaxResource2);
                getResourceValue(custom13ID, ref customResource1.CurrentResource3, ref customResource1.MaxResource3);
                getResourceValue(custom14ID, ref customResource1.CurrentResource4, ref customResource1.MaxResource4);
            }

            if (custom2Found)
            {
                getResourceValue(custom21ID, ref customResource2.CurrentResource1, ref customResource2.MaxResource1);
                getResourceValue(custom22ID, ref customResource2.CurrentResource2, ref customResource2.MaxResource2);
                getResourceValue(custom23ID, ref customResource2.CurrentResource3, ref customResource2.MaxResource3);
                getResourceValue(custom24ID, ref customResource2.CurrentResource4, ref customResource2.MaxResource4);
            }
        }

        // Return false if the values were not found and set the values to 0.
        private bool getResourceValue(int? ResourceID, ref float currentValue, ref float maxValue)
        {
            if(ResourceID != null && ARPWrapper.KSPARP.VesselResources.ContainsKey(ResourceID.Value))
            {
                maxValue = (float)ARPWrapper.KSPARP.VesselResources[ResourceID.Value].MaxAmountValue;
                currentValue = (float)ARPWrapper.KSPARP.VesselResources[ResourceID.Value].AmountValue;
                return true;
            }
            else
            {
                maxValue = 0;
                currentValue = 0;
                return false;
            }
        }

        public void ResourceProvider()
        {
            if (resourceChannel != null)
            {
                resourceChannel.Fire(OutboundPackets.TACLSResource, resourceStruct);
            }
        }

        public void WasteProvider()
        {
            if (wasteChannel != null)
            {
                wasteChannel.Fire(OutboundPackets.TACLSWaste, wasteStruct);
            }
        }

        public void Custom1Provider()
        {
            if (custom1Channel != null)
            {
                custom1Channel.Fire(OutboundPackets.CustomResource1, customResource1);
            }
        }

        public void Custom2Provider()
        {
            if (custom2Channel != null)
            {
                custom2Channel.Fire(OutboundPackets.CustomResource2, customResource2);
            }
        }

        private int? GetResourceID(string resourceName)
        {
            if(resourceName == "" || resourceName == "none" || resourceName.Length <= 1)
            {
                //Default values of the resourceName, in this case no resource is requested.
                return null;
            }

            PartResourceDefinition resource = null;
            try
            {
                resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            }
            catch (NullReferenceException)
            {
                Debug.Log("Simpit : I raised a NullReferenceException when looking for resource '" + resourceName + "'");
            }

            if (resource == null) {
                Debug.Log("Simpit : I'm looking for resource " + resourceName + " and I did *not* found it.");
                return null;
            } else
            {
                Debug.Log("Simpit : I'm looking for resource " + resourceName + " and I found it.");
                return resource.id;
            }
        }
    }
}
