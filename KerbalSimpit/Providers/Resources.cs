using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimpit.External;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitResourceProvider : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct ResourceStruct
        {
            public float Max;
            public float Available;
        }

        private ResourceStruct TotalLF, StageLF, TotalOx, StageOx,
            TotalSF, StageSF, TotalMono, TotalElectric, TotalEva,
            TotalOre, TotalAb, StageAb, TotalXenon, StageXenon;

        private EventData<byte, object> LFChannel, LFStageChannel,
            OxChannel, OxStageChannel, SFChannel, SFStageChannel,
            MonoChannel, ElectricChannel, EvaChannel, OreChannel,
            AbChannel, AbStageChannel, XenonChannel, XenonStageChannel;

        // IDs of resources we care about
        private int LiquidFuelID, OxidizerID, SolidFuelID,
            MonoPropellantID, ElectricChargeID, EvaPropellantID,
            OreID, AblatorID, XenonID;

        private bool ARPPresent;

        public void Start()
        {
            ARPWrapper.InitKSPARPWrapper();
            if (ARPWrapper.APIReady)
            {
                ARPPresent = true;
                KSPit.AddToDeviceHandler(LFProvider);
                LFChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial1");
                KSPit.AddToDeviceHandler(LFStageProvider);
                LFStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial2");
                KSPit.AddToDeviceHandler(OxProvider);
                OxChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial3");
                KSPit.AddToDeviceHandler(OxStageProvider);
                OxStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial4");
                KSPit.AddToDeviceHandler(SFProvider);
                SFChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial5");
                KSPit.AddToDeviceHandler(SFStageProvider);
                SFStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial6");
                KSPit.AddToDeviceHandler(XenonProvider);
                XenonChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial7");
                KSPit.AddToDeviceHandler(XenonStageProvider);
                XenonStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial8");
                KSPit.AddToDeviceHandler(MonoProvider);
                MonoChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial9");
                KSPit.AddToDeviceHandler(ElectricProvider);
                ElectricChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial20");
                KSPit.AddToDeviceHandler(EvaProvider);
                EvaChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial10");
                KSPit.AddToDeviceHandler(OreProvider);
                OreChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial21");
                KSPit.AddToDeviceHandler(AbProvider);
                AbChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial22");
                KSPit.AddToDeviceHandler(AbStageProvider);
                AbStageChannel =
                    GameEvents.FindEvent<EventData<byte, object>>("toSerial23");

                ScanForResources();
            } else {
                ARPPresent = false;
                Debug.Log("KerbalSimpit: AlternateResourcePanel not found. Resource providers WILL NOT WORK.");
            }
        }

        public void Update()
        {
            if (ARPPresent)
            {
                GetTotalResources(LiquidFuelID, ref TotalLF);
                GetStageResources(LiquidFuelID, ref StageLF);
                GetTotalResources(OxidizerID, ref TotalOx);
                GetStageResources(OxidizerID, ref StageOx);
                GetTotalResources(SolidFuelID, ref TotalSF);
                GetStageResources(SolidFuelID, ref StageSF);
                GetTotalResources(XenonID, ref TotalXenon);
                GetStageResources(XenonID, ref StageXenon);
                GetTotalResources(MonoPropellantID, ref TotalMono);
                GetTotalResources(ElectricChargeID, ref TotalElectric);
                GetTotalResources(EvaPropellantID, ref TotalEva);
                GetTotalResources(OreID, ref TotalOre);
                GetTotalResources(AblatorID, ref TotalAb);
                GetStageResources(AblatorID, ref StageAb);
            }
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(LFProvider);
            KSPit.RemoveToDeviceHandler(LFStageProvider);
            KSPit.RemoveToDeviceHandler(OxProvider);
            KSPit.RemoveToDeviceHandler(OxStageProvider);
            KSPit.RemoveToDeviceHandler(SFProvider);
            KSPit.RemoveToDeviceHandler(SFStageProvider);
            KSPit.RemoveToDeviceHandler(XenonProvider);
            KSPit.RemoveToDeviceHandler(XenonStageProvider);
            KSPit.RemoveToDeviceHandler(MonoProvider);
            KSPit.RemoveToDeviceHandler(ElectricProvider);
            KSPit.RemoveToDeviceHandler(EvaProvider);
            KSPit.RemoveToDeviceHandler(OreProvider);
            KSPit.RemoveToDeviceHandler(AbProvider);
            KSPit.RemoveToDeviceHandler(AbStageProvider);
        }

        public void ScanForResources()
        {
            if(KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Vessel changed, scanning for resrouces.");
            LiquidFuelID = GetResourceID("LiquidFuel");
            OxidizerID = GetResourceID("Oxidizer");
            SolidFuelID = GetResourceID("SolidFuel");
            XenonID = GetResourceID("XenonGas");
            MonoPropellantID = GetResourceID("MonoPropellant");
            ElectricChargeID = GetResourceID("ElectricCharge");
            EvaPropellantID = GetResourceID("EVA Propellant");
            OreID = GetResourceID("Ore");
            AblatorID = GetResourceID("Ablator");
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

        public void XenonProvider()
        {
            if (XenonChannel != null) XenonChannel.Fire(OutboundPackets.XenonGas, TotalXenon);
        }

        public void XenonStageProvider()
        {
            if (XenonStageChannel != null) XenonStageChannel.Fire(OutboundPackets.XenonGasStage, StageXenon);
        }

        public void MonoProvider()
        {
            if (MonoChannel != null) MonoChannel.Fire(OutboundPackets.MonoPropellant, TotalMono);
        }

        public void ElectricProvider()
        {
            if (ElectricChannel != null) ElectricChannel.Fire(OutboundPackets.ElectricCharge, TotalElectric);
        }

        public void EvaProvider()
        {
            if (EvaChannel != null) EvaChannel.Fire(OutboundPackets.EvaPropellant, TotalEva);
        }

        public void OreProvider()
        {
            if (OreChannel != null) OreChannel.Fire(OutboundPackets.Ore, TotalOre);
        }

        public void AbProvider()
        {
            if (AbChannel != null) AbChannel.Fire(OutboundPackets.Ablator, TotalAb);
        }

        public void AbStageProvider()
        {
            if (AbStageChannel != null) AbStageChannel.Fire(OutboundPackets.AblatorStage, StageAb);
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

