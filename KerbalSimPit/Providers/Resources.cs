using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimPit.External;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimPitResourceProvider : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct ResourceStruct
        {
            public float Max;
            public float Available;
        }

        private ResourceStruct TotalLF, StageLF;

        private EventData<byte, object> LFChannel, LFStageChannel;

        // IDs of resources we care about
        private int LiquidFuelID;
        //private int LiquidFuelID, OxidizerID, SolidFuelID,
        //    MonoPropellantID, ElectricChargeID, EvaPropellantID,
        //    OreID, AblatorID;

        /* Current plan for integrating ARP:
           Look up the resources I care about in the
           ResourceDefinitionLibrary, and store their IDs.
           In Update, check for those IDs in the KSPARP properties
           for the vessel and stage resources.
           Should also subscribe to vessel change updates and use
           those to refresh the ID cache.
        */
        public void Start()
        {
            ARPWrapper.InitKSPARPWrapper();
            if (ARPWrapper.APIReady)
            {
                KSPit.AddToDeviceHandler(LFProvider);
                LFChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial10");
                KSPit.AddToDeviceHandler(LFStageProvider);
                LFStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial11");
                ScanForResources();
            } else {
                Debug.Log("KerbalSimPit: AlternateResourcePanel not found. Resource providers WILL NOT WORK.");
            }
            ScanForResources();
        }

        public void Update()
        {
            if (ARPWrapper.KSPARP.VesselResources.ContainsKey(LiquidFuelID))
            {
                TotalLF.Max = (float)ARPWrapper.KSPARP.VesselResources[LiquidFuelID].MaxAmountValue;
                TotalLF.Available = (float)ARPWrapper.KSPARP.VesselResources[LiquidFuelID].AmountValue;
                StageLF.Max = (float)ARPWrapper.KSPARP.LastStageResources[LiquidFuelID].MaxAmountValue;
                StageLF.Available = (float)ARPWrapper.KSPARP.LastStageResources[LiquidFuelID].AmountValue;
            } else {
                TotalLF.Max = 0;
                TotalLF.Available = 0;
                StageLF.Max = 0;
                StageLF.Available = 0;
            }
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(LFProvider);
            KSPit.RemoveToDeviceHandler(LFStageProvider);
        }

        public void ScanForResources()
        {
            if(KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Vessel changed, scanning for resrouces.");
            LiquidFuelID = GetResourceID("LiquidFuel");
            // OxidizerID = GetResourceID("Oxidizer");
            // SolidFuelID = GetResourceID("SolidFuel");
            // MonoPropellantID = GetResourceID("MonoPropellant");
            // ElectricChargeID = GetResourceID("ElectricCharge");
            // EvaPropellantID = GetResourceID("EvaPropellant");
            // OreID = GetResourceID("Ore");
            // AblatorID = GetResourceID("Ablator");
        }

        public void LFProvider()
        {
            Debug.Log(String.Format("KerbalSimPit: Sending Liquid Fuel max {0} cur {1}", TotalLF.Max, TotalLF.Available));
            if (LFChannel != null) LFChannel.Fire(OutboundPackets.LiquidFuel, TotalLF);
        }

        public void LFStageProvider()
        {
            Debug.Log(String.Format("KerbalSimPit: Sending Liquid Fuel Stage max {0} cur{1}", StageLF.Max, StageLF.Available));
            if (LFStageChannel != null) LFStageChannel.Fire(OutboundPackets.LiquidFuelStage, StageLF);
        }

        private int GetResourceID(string resourceName)
        {
            PartResourceDefinition resource =
                PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }
    }
}

