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

        private ResourceStruct TotalLF, StageLF, TotalOx, StageOx,
            TotalSF, StageSF;

        private EventData<byte, object> LFChannel, LFStageChannel,
            OxChannel, OxStageChannel, SFChannel, SFStageChannel;

        // IDs of resources we care about
        private int LiquidFuelID, OxidizerID, SolidFuelID;
        //private int LiquidFuelID, OxidizerID, SolidFuelID,
        //    MonoPropellantID, ElectricChargeID, EvaPropellantID,
        //    OreID, AblatorID;

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
                KSPit.AddToDeviceHandler(OxStageProvider);
                OxStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial13");
                KSPit.AddToDeviceHandler(SFProvider);
                SFChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial14");
                KSPit.AddToDeviceHandler(SFStageProvider);
                SFStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial15");

                ScanForResources();
            } else {
                Debug.Log("KerbalSimPit: AlternateResourcePanel not found. Resource providers WILL NOT WORK.");
            }
            ScanForResources();
        }

        public void Update()
        {
            GetTotalResources(LiquidFuelID, ref TotalLF);
            GetStageResources(LiquidFuelID, ref StageLF);
            GetTotalResources(OxidizerID, ref TotalOx);
            GetStageResources(OxidizerID, ref StageOx);
            GetTotalResources(SolidFuelID, ref TotalSF);
            GetStageResources(SolidFuelID, ref StageSF);
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(LFProvider);
            KSPit.RemoveToDeviceHandler(LFStageProvider);
            KSPit.RemoveToDeviceHandler(OxProvider);
            KSPit.RemoveToDeviceHandler(OxStageProvider);
            KSPit.RemoveToDeviceHandler(SFProvider);
            KSPit.RemoveToDeviceHandler(SFStageProvider);
        }

        public void ScanForResources()
        {
            if(KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Vessel changed, scanning for resrouces.");
            LiquidFuelID = GetResourceID("LiquidFuel");
            OxidizerID = GetResourceID("Oxidizer");
            SolidFuelID = GetResourceID("SolidFuel");
            // MonoPropellantID = GetResourceID("MonoPropellant");
            // ElectricChargeID = GetResourceID("ElectricCharge");
            // EvaPropellantID = GetResourceID("EvaPropellant");
            // OreID = GetResourceID("Ore");
            // AblatorID = GetResourceID("Ablator");
        }

        public void LFProvider()
        {
            if (LFChannel != null) LFChannel.Fire(OutboundPackets.LiquidFuel, TotalLF);
        }

        public void LFStageProvider()
        {
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

        public void SFProvider()
        {
            if (SFChannel != null) SFChannel.Fire(OutboundPackets.SolidFuel, TotalSF);
        }

        public void SFStageProvider()
        {
            if (SFStageChannel != null) SFStageChannel.Fire(OutboundPackets.SolidFuelStage, StageSF);
        }

        private int GetResourceID(string resourceName)
        {
            PartResourceDefinition resource =
                PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }

        private bool GetTotalResources(int ResourceID, ref ResourceStruct DestResourceStruct)
        {
            if (ARPWrapper.KSPARP.VesselResources.ContainsKey(ResourceID))
            {
                DestResourceStruct.Max = (float)ARPWrapper.KSPARP.VesselResources[ResourceID].MaxAmountValue;
                DestResourceStruct.Available = (float)ARPWrapper.KSPARP.VesselResources[ResourceID].AmountValue;
                return true;
            } else {
                DestResourceStruct.Max = 0;
                DestResourceStruct.Available = 0;
                return false;
            }
        }

        private bool GetStageResources(int ResourceID, ref ResourceStruct DestResourceStruct)
        {
            if (ARPWrapper.KSPARP.LastStageResources.ContainsKey(ResourceID))
            {
                DestResourceStruct.Max = (float)ARPWrapper.KSPARP.LastStageResources[ResourceID].MaxAmountValue;
                DestResourceStruct.Available = (float)ARPWrapper.KSPARP.LastStageResources[ResourceID].AmountValue;
                return true;
            } else {
                DestResourceStruct.Max = 0;
                DestResourceStruct.Available = 0;
                return false;
            }
        }
    }
}

