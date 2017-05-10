using System;
using System.Runtime.InteropServices;
using KSP.IO;
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

        private ResourceStruct myTotalLF;

        private EventData<byte, object> liquidFuelChannel;

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

            KSPit.AddToDeviceHandler(LiquidFuelProvider);
            liquidFuelChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial10");

            if (ARPWrapper.APIReady)
            {
                Debug.Log(String.Format("ARPWrapperTest: {0} resources found",
                                        ARPWrapper.KSPARP.VesselResources.Count));
            } else {
                Debug.Log("KerbalSimPit: AlternateResourcePanel not found. Resource providers WILL NOT WORK.");
            }
            ScanForResources();
        }

        public void Update()
        {
            if (ARPWrapper.KSPARP.VesselResources.ContainsKey(LiquidFuelID))
            {
                myTotalLF.Max = (float)ARPWrapper.KSPARP.VesselResources[LiquidFuelID].MaxAmountValue;
                myTotalLF.Available = (float)ARPWrapper.KSPARP.VesselResources[LiquidFuelID].AmountValue;
            } else {
                myTotalLF.Max = 0;
                myTotalLF.Available = 0;
            }
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(LiquidFuelProvider);
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

        public void LiquidFuelProvider()
        {
            Debug.Log(String.Format("KerbalSimPit: Sending Liquid Fuel max {0} cur {1}", myTotalLF.Max, myTotalLF.Available));
            if (liquidFuelChannel != null) liquidFuelChannel.Fire(OutboundPackets.LiquidFuel, myTotalLF);
        }

        private int GetResourceID(string resourceName)
        {
            PartResourceDefinition resource =
                PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }
    }
}

