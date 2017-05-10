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

        private ResourceStruct TotalLF, StageLF, TotalOx, StageOx;

        private EventData<byte, object> LFChannel, LFStageChannel,
            OxChannel, OxStageChannel;

        // IDs of resources we care about
        private int LiquidFuelID, OxidizerID;
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
                KSPit.AddToDeviceHandler(OxProvider);
                OxChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial12");
                OxStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial13");

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
            } else {
                TotalLF.Max = 0;
                TotalLF.Available = 0;
            }
            if (ARPWrapper.KSPARP.LastStageResources.ContainsKey(LiquidFuelID))
            {
                StageLF.Max = (float)ARPWrapper.KSPARP.LastStageResources[LiquidFuelID].MaxAmountValue;
                StageLF.Available = (float)ARPWrapper.KSPARP.LastStageResources[LiquidFuelID].AmountValue;
            } else {
                StageLF.Max = 0;
                StageLF.Available = 0;
            }

            if (ARPWrapper.KSPARP.VesselResources.ContainsKey(OxidizerID))
            {
                TotalOx.Max = (float)ARPWrapper.KSPARP.VesselResources[OxidizerID].MaxAmountValue;
                TotalOx.Available = (float)ARPWrapper.KSPARP.VesselResources[OxidizerID].AmountValue;
            } else {
                TotalOx.Max = 0;
                TotalOx.Available = 0;
            }

            if (ARPWrapper.KSPARP.LastStageResources.ContainsKey(OxidizerID))
            {
                StageOx.Max = (float)ARPWrapper.KSPARP.LastStageResources[OxidizerID].MaxAmountValue;
                StageOx.Available = (float)ARPWrapper.KSPARP.VesselResources[OxidizerID].AmountValue;
            } else {
                StageOx.Max = 0;
                StageOx.Available = 0;
            }
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(LFProvider);
            KSPit.RemoveToDeviceHandler(LFStageProvider);
            KSPit.RemoveToDeviceHandler(OxProvider);
            KSPit.RemoveToDeviceHandler(OxStageProvider);
        }

        public void ScanForResources()
        {
            if(KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Vessel changed, scanning for resrouces.");
            LiquidFuelID = GetResourceID("LiquidFuel");
            OxidizerID = GetResourceID("Oxidizer");
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

        public void OxProvider()
        {
            if (OxChannel != null) OxChannel.Fire(OutboundPackets.Oxidizer, TotalOx);
        }

        public void OxStageProvider()
        {
            if (OxStageChannel != null) OxStageChannel.Fire(OutboundPackets.OxidizerStage, StageOx);
        }

        private int GetResourceID(string resourceName)
        {
            PartResourceDefinition resource =
                PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }
    }
}

