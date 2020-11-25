using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitTelemetryProvider : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct AltitudeStruct
        {
            public float alt;
            public float surfalt;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct ApsidesStruct
        {
            public float periapsis;
            public float apoapsis;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct ApsidesTimeStruct
        {
            public int periapsis;
            public int apoapsis;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct VelocityStruct
        {
            public float orbital;
            public float surface;
            public float vertical;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct AirspeedStruct
        {
            public float IAS;
            public float MachNumber;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)][Serializable]
        public struct DeltaVStruct
        {
            public float stageDeltaV;
            public float totalDeltaV;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)][Serializable]
        public struct DeltaVEnvStruct
        {
            public float stageDeltaVASL;
            public float totalDeltaVASL;
            public float stageDeltaVVac;
            public float totalDeltaVVac;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)][Serializable]
        public struct BurnTimeStruct
        {
            public float stageBurnTime;
            public float totalBurnTime;
        }

        private AltitudeStruct myAlt;
        private ApsidesStruct myApsides;
        private ApsidesTimeStruct myApsidesTime;
        private VelocityStruct myVelocity;
        private AirspeedStruct myAirspeed;
        private DeltaVStruct myDeltaVStruct;
        private DeltaVEnvStruct myDeltaVEnvStruct;
        private BurnTimeStruct myBurnTimeStruct;

        private EventData<byte, object> altitudeChannel, apsidesChannel,
            apsidesTimeChannel, velocityChannel, soiChannel, airspeedChannel,
            deltaVChannel, deltaVEnvChannel, burnTimeChannel;

        private string CurrentSoI;

        public void Start()
        {
            KSPit.AddToDeviceHandler(AltitudeProvider);
            altitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial30");
            KSPit.AddToDeviceHandler(ApsidesProvider);
            apsidesChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial33");
            KSPit.AddToDeviceHandler(ApsidesTimeProvider);
            apsidesTimeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial34");
            KSPit.AddToDeviceHandler(VelocityProvider);
            velocityChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial31");
            KSPit.AddToDeviceHandler(DeltaVProvider);
            deltaVChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial41");
            KSPit.AddToDeviceHandler(DeltaVEnvProvider);
            deltaVEnvChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial42");
            KSPit.AddToDeviceHandler(BurnTimeProvider);
            burnTimeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial43");
            // We fire one SoI packet when SoI changes. So no need to use the
            // periodic DeviceHandler infrastructure.
            CurrentSoI = "";
            soiChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial51");
            KSPit.AddToDeviceHandler(AirspeedProvider);
            airspeedChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial32");
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(AltitudeProvider);
            KSPit.RemoveToDeviceHandler(ApsidesProvider);
            KSPit.RemoveToDeviceHandler(ApsidesTimeProvider);
            KSPit.RemoveToDeviceHandler(VelocityProvider);
            KSPit.RemoveToDeviceHandler(AirspeedProvider);
            KSPit.RemoveToDeviceHandler(DeltaVProvider);
            KSPit.RemoveToDeviceHandler(DeltaVEnvProvider);
            KSPit.RemoveToDeviceHandler(BurnTimeProvider);
        }

        public void Update()
        {
            if (soiChannel != null)
            {
                if (FlightGlobals.ActiveVessel.orbit.referenceBody.bodyName != CurrentSoI)
                {
                    CurrentSoI = FlightGlobals.ActiveVessel.orbit.referenceBody.bodyName;
                    soiChannel.Fire(OutboundPackets.SoIName, Encoding.ASCII.GetBytes(CurrentSoI));
                }
            }
        }

        public void AltitudeProvider()
        {
            myAlt.alt = (float)FlightGlobals.ActiveVessel.altitude;
            myAlt.surfalt = (float)FlightGlobals.ActiveVessel.radarAltitude;
            if (altitudeChannel != null) altitudeChannel.Fire(OutboundPackets.Altitude, myAlt);
        }

        public void ApsidesProvider()
        {
            myApsides.apoapsis = (float)FlightGlobals.ActiveVessel.orbit.ApA;
            myApsides.periapsis = (float)FlightGlobals.ActiveVessel.orbit.PeA;
            if (apsidesChannel != null) apsidesChannel.Fire(OutboundPackets.Apsides, myApsides);
        }

        public void ApsidesTimeProvider()
        {
            myApsidesTime.apoapsis = (int)FlightGlobals.ActiveVessel.orbit.timeToAp;
            myApsidesTime.periapsis = (int)FlightGlobals.ActiveVessel.orbit.timeToPe;
            if (apsidesTimeChannel != null) apsidesTimeChannel.Fire(OutboundPackets.ApsidesTime, myApsidesTime);
        }

        public void VelocityProvider()
        {
            myVelocity.orbital = (float)FlightGlobals.ActiveVessel.obt_speed;
            myVelocity.surface = (float)FlightGlobals.ActiveVessel.srfSpeed;
            myVelocity.vertical = (float)FlightGlobals.ActiveVessel.verticalSpeed;
            if (velocityChannel != null) velocityChannel.Fire(OutboundPackets.Velocities, myVelocity);
        }

        public void AirspeedProvider()
        {
            myAirspeed.IAS = (float)FlightGlobals.ActiveVessel.indicatedAirSpeed;
            myAirspeed.MachNumber = (float)FlightGlobals.ActiveVessel.mach;
            if (airspeedChannel != null) airspeedChannel.Fire(OutboundPackets.Airspeed, myAirspeed);
        }

        //Return the DeltaVStageInfo of the first stage to consider for deltaV and burn time computation
        private DeltaVStageInfo getCurrentStageDeltaV()
        {
            DeltaVStageInfo currentStageInfo = null;
            if (FlightGlobals.ActiveVessel.currentStage == FlightGlobals.ActiveVessel.VesselDeltaV.OperatingStageInfo.Count)
            {
                // Rocket has not taken off, use first stage with deltaV (to avoid stage of only stabilizer)
                for (int i = FlightGlobals.ActiveVessel.VesselDeltaV.OperatingStageInfo.Count - 1; i >= 0; i--)
                {
                    currentStageInfo = FlightGlobals.ActiveVessel.VesselDeltaV.GetStage(i);
                    if (currentStageInfo.deltaVActual > 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                currentStageInfo = FlightGlobals.ActiveVessel.VesselDeltaV.GetStage(FlightGlobals.ActiveVessel.currentStage);
            }

            return currentStageInfo;
        }

        public void DeltaVProvider()
        {
            DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();

            if (currentStageInfo != null)
            {
                myDeltaVStruct.stageDeltaV = (float)currentStageInfo.deltaVActual;
            }
            else
            {
                //Debug.Log("KerbalSimpit: DeltaVProvider could not find current stage info.");
                myDeltaVStruct.stageDeltaV = 0;
            }
            myDeltaVStruct.totalDeltaV = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalDeltaVActual;
            if (deltaVChannel != null) deltaVChannel.Fire(OutboundPackets.DeltaV, myDeltaVStruct);
        }

        public void DeltaVEnvProvider()
        {
            DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();

            if (currentStageInfo != null)
            {

                myDeltaVEnvStruct.stageDeltaVASL = (float)currentStageInfo.deltaVatASL;
                myDeltaVEnvStruct.stageDeltaVVac = (float)currentStageInfo.deltaVinVac;
            }
            else
            {
                //Debug.Log("KerbalSimpit: DeltaVEnvProvider could not find current stage info.");
                myDeltaVEnvStruct.stageDeltaVASL = 0;
                myDeltaVEnvStruct.stageDeltaVVac = 0;
            }

            myDeltaVEnvStruct.totalDeltaVASL = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalDeltaVASL;
            myDeltaVEnvStruct.totalDeltaVVac = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalDeltaVVac;
            if (deltaVEnvChannel != null) deltaVEnvChannel.Fire(OutboundPackets.DeltaVEnv, myDeltaVEnvStruct);
        }

        public void BurnTimeProvider()
        {
            DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();

            if (currentStageInfo != null)
            {
                myBurnTimeStruct.stageBurnTime = (float)currentStageInfo.stageBurnTime;
            }
            else
            {
                //Debug.Log("KerbalSimpit: BurnTimeProvider could not find current stage info.");
                myBurnTimeStruct.stageBurnTime = 0;
            }

            myBurnTimeStruct.totalBurnTime = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalBurnTime;
            if (burnTimeChannel != null) burnTimeChannel.Fire(OutboundPackets.BurnTime, myBurnTimeStruct);
        }
    }
}
