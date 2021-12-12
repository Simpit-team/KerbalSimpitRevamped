using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimpit.External;
using KerbalSimpit.KerbalSimpit.Providers;

namespace KerbalSimpit.Providers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct ResourceStruct
    {
        public float Max;
        public float Available;
    }

    /// <summary>
    /// Generic provider for a resource message.
    /// This abstract class need only a default constructor to be usable, to define the channel ID,
    /// the resource name and if the computed is performed on the whole vessel or for the current stage only.
    /// </summary>
    abstract class GenericResourceProvider : GenericProvider<ResourceStruct>
    {
        private int _resourceID;
        private bool _stageOnly;
        private bool _isARPInstalled;

        public GenericResourceProvider(byte channelID, string resourceName, bool stageOnly) : base(channelID)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            _resourceID = resource.id;
            _stageOnly = stageOnly;
            _isARPInstalled = true;

            if (!ARPWrapper.APIReady)
            {
                ARPWrapper.InitKSPARPWrapper();
            }
            if (!ARPWrapper.APIReady)
            {
                Debug.Log("KerbalSimpit: AlternateResourcePanel not found. Resource providers WILL NOT WORK.");
                _isARPInstalled = false;
            }
        }

        override protected bool updateMessage(ref ResourceStruct message)
        {

            if (_isARPInstalled && _stageOnly && ARPWrapper.KSPARP.LastStageResources.ContainsKey(_resourceID))
            {
                message.Max = (float)ARPWrapper.KSPARP.LastStageResources[_resourceID].MaxAmountValue;
                message.Available = (float)ARPWrapper.KSPARP.LastStageResources[_resourceID].AmountValue;
            }
            else if (_isARPInstalled && !_stageOnly && ARPWrapper.KSPARP.VesselResources.ContainsKey(_resourceID))
            {
                message.Max = (float)ARPWrapper.KSPARP.VesselResources[_resourceID].MaxAmountValue;
                message.Available = (float)ARPWrapper.KSPARP.VesselResources[_resourceID].AmountValue;
            }
            else
            {
                message.Max = 0;
                message.Available = 0;
            }
            return false;
        }
    }

    class LiquidFuelProvider : GenericResourceProvider { public LiquidFuelProvider() : base(OutboundPackets.LiquidFuel, "LiquidFuel", false) { } }
    class LiquidFuelStageProvider : GenericResourceProvider { public LiquidFuelStageProvider() : base(OutboundPackets.LiquidFuelStage, "LiquidFuel", true) { } }
    class OxidizerProvider : GenericResourceProvider { public OxidizerProvider() : base(OutboundPackets.Oxidizer, "Oxidizer", false) { } }
    class OxidizerStageProvider : GenericResourceProvider { public OxidizerStageProvider() : base(OutboundPackets.OxidizerStage, "Oxidizer", true) { } }
    class SolidFuelProvider : GenericResourceProvider { public SolidFuelProvider() : base(OutboundPackets.SolidFuel, "SolidFuel", false) { } }
    class SolidFuelStageProvider : GenericResourceProvider { public SolidFuelStageProvider() : base(OutboundPackets.SolidFuelStage, "SolidFuel", true) { } }
    class MonoPropellantProvider : GenericResourceProvider { public MonoPropellantProvider() : base(OutboundPackets.MonoPropellant, "MonoPropellant", false) { } }
    class ElectricChargeProvider : GenericResourceProvider { public ElectricChargeProvider() : base(OutboundPackets.ElectricCharge, "ElectricCharge", false) { } }
    class OreProvider : GenericResourceProvider { public OreProvider() : base(OutboundPackets.Ore, "Ore", false) { } }
    class AblatorProvider : GenericResourceProvider { public AblatorProvider() : base(OutboundPackets.Ablator, "Ablator", false) { } }
    class AblatorStageProvider : GenericResourceProvider { public AblatorStageProvider() : base(OutboundPackets.AblatorStage, "Ablator", true) { } }
    class XenonGasProvider : GenericResourceProvider { public XenonGasProvider() : base(OutboundPackets.XenonGas, "XenonGas", false) { } }
    class XenonGasStageProvider : GenericResourceProvider { public XenonGasStageProvider() : base(OutboundPackets.XenonGasStage, "XenonGas", true) { } }


    /** Since 1.11, ARP does not work correctly when in EVA. So this is a special case for EVA fuel, only available during EVA,
     *  to get around this issue
     */
    class EVAProvider : GenericProvider<ResourceStruct>
    {
        EVAProvider() : base(OutboundPackets.EvaPropellant) { }

        override protected bool updateMessage(ref ResourceStruct message)
        {
            message.Available = 0;
            message.Max = 0;
            if (FlightGlobals.ActiveVessel == null) return false;

            if (FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.evaController != null)
            {
                message.Max = (float)FlightGlobals.ActiveVessel.evaController.FuelCapacity;
                message.Available = (float)FlightGlobals.ActiveVessel.evaController.Fuel;
            }

            return false;
        }
    }
}
