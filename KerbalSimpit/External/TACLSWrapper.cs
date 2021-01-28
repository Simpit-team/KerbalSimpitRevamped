using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.External
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class TACLSWrapper : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct TACLSRessourceStruct
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

        private TACLSRessourceStruct ressourceStruct;
        private TACLSWasteStruct wasteStruct;

        private EventData<byte, object> ressourceChannel, wasteChannel;

        // IDs of resources we care about
        private int FoodID, WaterID, OxygenID, WasteID, LiquidWasteID, CO2ID;

        bool TASLSFound;

        public void Start()
        {
            ARPWrapper.InitKSPARPWrapper();

            if (ARPWrapper.APIReady)
            {
                FoodID = GetResourceID("Food");
                WaterID = GetResourceID("Water");
                OxygenID = GetResourceID("Oxygen");
                WasteID = GetResourceID("Waste");
                LiquidWasteID = GetResourceID("WasteWater");
                CO2ID = GetResourceID("CarbonDioxide");

                if(FoodID != -1 && WaterID != -1 && OxygenID != -1 && WasteID != -1 && LiquidWasteID != -1 && CO2ID != -1)
                {
                    KSPit.AddToDeviceHandler(RessourceProvider);
                    ressourceChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TACLSRessource);
                    KSPit.AddToDeviceHandler(WasteProvider);
                    wasteChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TACLSWaste);

                    TASLSFound = true; 
                    Debug.Log("KerbalSimpit: TACLS ressources found. You can subscribe to the TACLS channels.");
                }
                else
                {
                    Debug.Log("KerbalSimpit: TACLS ressources not found. I assume TACLS is not installed.");
                    TASLSFound = false;
                }

            } else
            {
                TASLSFound = false;
            }
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(RessourceProvider);
            KSPit.RemoveToDeviceHandler(WasteProvider);
        }

        public void Update()
        {
            if (TASLSFound)
            {
                getRessourceValue(FoodID, ref ressourceStruct.CurrentFood, ref ressourceStruct.MaxFood);
                getRessourceValue(WaterID, ref ressourceStruct.CurrentWater, ref ressourceStruct.MaxWater);
                getRessourceValue(OxygenID, ref ressourceStruct.CurrentOxygen, ref ressourceStruct.MaxOxygen);

                getRessourceValue(WasteID, ref wasteStruct.CurrentWaste, ref wasteStruct.MaxWaste);
                getRessourceValue(LiquidWasteID, ref wasteStruct.CurrentLiquidWaste, ref wasteStruct.MaxLiquidWaste);
                getRessourceValue(CO2ID, ref wasteStruct.CurrentCO2, ref wasteStruct.MaxCO2);
            }
        }

        // Return false if the values were not found
        private bool getRessourceValue(int ResourceID, ref float currentValue, ref float maxValue)
        {
            if (ARPWrapper.KSPARP.VesselResources.ContainsKey(ResourceID))
            {
                maxValue = (float)ARPWrapper.KSPARP.VesselResources[ResourceID].MaxAmountValue;
                currentValue = (float)ARPWrapper.KSPARP.VesselResources[ResourceID].AmountValue;
                return false;
            }
            else
            {
                maxValue = 0;
                currentValue = 0;
                return false;
            }
        }

        public void RessourceProvider()
        {
            if (ressourceChannel != null)
            {
                ressourceChannel.Fire(OutboundPackets.TACLSRessource, ressourceStruct);
            }
        }

        public void WasteProvider()
        {
            if (wasteChannel != null)
            {
                wasteChannel.Fire(OutboundPackets.TACLSWaste, wasteStruct);
            }
        }

        // Returns -1 if the ressource is not found (for instance if TACLS is not found)
        private int GetResourceID(string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            if (resource == null) { 
                return -1;
            } else { 
                return resource.id;
            }
        }
    }
}
