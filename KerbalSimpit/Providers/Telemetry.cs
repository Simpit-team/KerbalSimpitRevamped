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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct OrbitInfoStruct
        {
            public float eccentricity;
            public float semiMajorAxis;
            public float inclination;
            public float longAscendingNode;
            public float argPeriapsis;
            public float trueAnomaly;
            public float meanAnomaly;
            public float period;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct AirspeedStruct
        {
            public float IAS;
            public float MachNumber;
        }

        public struct ManeuverStruct
        {
            public float timeToNextManeuver;
            public float deltaVNextManeuver;
            public float durationNextManeuver;
            public float deltaVTotal;
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct TempLimitStruct
        {
            public byte tempLimitPercentage;
            public byte skinTempLimitPercentage;
        }

        private AltitudeStruct myAlt;
        private ApsidesStruct myApsides;
        private ApsidesTimeStruct myApsidesTime;
        private VelocityStruct myVelocity;
        private AirspeedStruct myAirspeed;
        private ManeuverStruct myManeuver;
        private DeltaVStruct myDeltaVStruct;
        private DeltaVEnvStruct myDeltaVEnvStruct;
        private BurnTimeStruct myBurnTimeStruct;
        private OrbitInfoStruct myOrbitInfoStruct;
        private TempLimitStruct myTempLimitStruct;

        private EventData<byte, object> altitudeChannel, apsidesChannel,
            apsidesTimeChannel, ortbitInfoChannel, velocityChannel, soiChannel, airspeedChannel,
            maneuverChannel, deltaVChannel, deltaVEnvChannel, burnTimeChannel, tempLimitChannel;

        private string CurrentSoI;

        public void Start()
        {
            KSPit.AddToDeviceHandler(AltitudeProvider);
            altitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial30");
            KSPit.AddToDeviceHandler(ApsidesProvider);
            apsidesChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial33");
            KSPit.AddToDeviceHandler(ApsidesTimeProvider);
            apsidesTimeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial34");
            KSPit.AddToDeviceHandler(OrbitInfoProvider);
            ortbitInfoChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.OrbitInfo);
            KSPit.AddToDeviceHandler(VelocityProvider);
            maneuverChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial35");
            KSPit.AddToDeviceHandler(ManeuverProvider);
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
            soiChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.SoIName);
            //When SOIName channel is subscribed to, we need to resend the SOI name.
            GameEvents.FindEvent<EventData<byte, object>>("onSerialChannelSubscribed" + OutboundPackets.SoIName).Add(resendSOI);
            KSPit.AddToDeviceHandler(AirspeedProvider);
            airspeedChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial32");
            KSPit.AddToDeviceHandler(TempLimitProvider);
            tempLimitChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TempLimit);
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(AltitudeProvider);
            KSPit.RemoveToDeviceHandler(ApsidesProvider);
            KSPit.RemoveToDeviceHandler(ApsidesTimeProvider);
            KSPit.RemoveToDeviceHandler(OrbitInfoProvider);
            KSPit.RemoveToDeviceHandler(VelocityProvider);
            KSPit.RemoveToDeviceHandler(AirspeedProvider);
            KSPit.RemoveToDeviceHandler(ManeuverProvider);
            KSPit.RemoveToDeviceHandler(DeltaVProvider);
            KSPit.RemoveToDeviceHandler(DeltaVEnvProvider);
            KSPit.RemoveToDeviceHandler(BurnTimeProvider);
            KSPit.RemoveToDeviceHandler(TempLimitProvider);
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

        public void resendSOI(byte ID, object Data)
        {
            CurrentSoI = "";
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

        public void OrbitInfoProvider()
        {
            Orbit currentOrbit = FlightGlobals.ActiveVessel.orbit;
            myOrbitInfoStruct.eccentricity = (float) currentOrbit.eccentricity;
            myOrbitInfoStruct.semiMajorAxis = (float)currentOrbit.semiMajorAxis;
            myOrbitInfoStruct.inclination = (float)currentOrbit.inclination;
            myOrbitInfoStruct.longAscendingNode = (float)currentOrbit.LAN;
            myOrbitInfoStruct.argPeriapsis = (float)currentOrbit.argumentOfPeriapsis;
            myOrbitInfoStruct.trueAnomaly = (float)currentOrbit.trueAnomaly;
            myOrbitInfoStruct.meanAnomaly = (float)currentOrbit.meanAnomaly;
            myOrbitInfoStruct.period = (float)currentOrbit.period;
            if (ortbitInfoChannel != null) ortbitInfoChannel.Fire(OutboundPackets.OrbitInfo, myOrbitInfoStruct);
        }

        public void AirspeedProvider()
        {
            myAirspeed.IAS = (float)FlightGlobals.ActiveVessel.indicatedAirSpeed;
            myAirspeed.MachNumber = (float)FlightGlobals.ActiveVessel.mach;
            if (airspeedChannel != null) airspeedChannel.Fire(OutboundPackets.Airspeed, myAirspeed);
        }

        public void TempLimitProvider()
        {
            double maxTempPercentage = 0.0;
            double maxSkinTempPercentage = 0.0;

            // Iterate on a copy ?
            foreach (Part part in FlightGlobals.ActiveVessel.Parts)
            {
                maxTempPercentage = Math.Max(maxTempPercentage, 100.0 * part.temperature / part.maxTemp);
                maxSkinTempPercentage = Math.Max(maxSkinTempPercentage, 100.0 * part.skinTemperature / part.skinMaxTemp);
            }

            //Prevent the byte to overflow in case of extremely hot vessel
            if (maxTempPercentage > 255) maxTempPercentage = 255;
            if (maxSkinTempPercentage > 255) maxSkinTempPercentage = 255;

            myTempLimitStruct.tempLimitPercentage = (byte)Math.Round(maxTempPercentage);
            myTempLimitStruct.skinTempLimitPercentage = (byte)Math.Round(maxSkinTempPercentage);

            if (tempLimitChannel != null) tempLimitChannel.Fire(OutboundPackets.TempLimit, myTempLimitStruct);
        }

        //Return the DeltaVStageInfo of the first stage to consider for deltaV and burn time computation
        //Can return null when no deltaV is available (for instance in EVA).
        private DeltaVStageInfo getCurrentStageDeltaV()
        {
            if(FlightGlobals.ActiveVessel.VesselDeltaV == null)
            {
                return null; //This happen in EVA for instance.
            }
            DeltaVStageInfo currentStageInfo = null;

            try
            {
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
            }
            catch (NullReferenceException)
            {
                // This happens when reverting a flight.
                // FlightGlobals.ActiveVessel.VesselDeltaV.OperatingStageInfo is not null but using it produce a
                // NullReferenceException in KSP code. This is probably due to the fact that the rocket is not fully initialized.
            }

            return currentStageInfo;
        }

        public void ManeuverProvider()
        {
            myManeuver.timeToNextManeuver = 0.0f;
            myManeuver.deltaVNextManeuver = 0.0f;
            myManeuver.durationNextManeuver = 0.0f;
            myManeuver.deltaVTotal = 0.0f;

            if (FlightGlobals.ActiveVessel.patchedConicSolver != null)
            {
                if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes != null)
                {
                    System.Collections.Generic.List<ManeuverNode> maneuvers = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes;

                    if(maneuvers.Count > 0)
                    {
                        myManeuver.timeToNextManeuver = (float)(maneuvers[0].UT - Planetarium.GetUniversalTime());
                        myManeuver.deltaVNextManeuver = (float)maneuvers[0].DeltaV.magnitude;

                        DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();
                        if (currentStageInfo != null)
                        {
                            //For now, use a simple crossmultiplication to compute the estimated burn time based on the current stage only
                            myManeuver.durationNextManeuver = (float)(maneuvers[0].DeltaV.magnitude * currentStageInfo.stageBurnTime) / currentStageInfo.deltaVActual;
                        }

                        foreach (ManeuverNode maneuver in maneuvers)
                        {
                            myManeuver.deltaVTotal += (float)maneuver.DeltaV.magnitude;
                        }
                    }
                }
            }
            if (maneuverChannel != null) maneuverChannel.Fire(OutboundPackets.ManeuverData, myManeuver);
        }

        public void DeltaVProvider()
        {
            DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();

            if (currentStageInfo != null)
            {
                myDeltaVStruct.stageDeltaV = (float)currentStageInfo.deltaVActual;
                myDeltaVStruct.totalDeltaV = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalDeltaVActual;
            }
            else
            {
                myDeltaVStruct.stageDeltaV = 0;
                myDeltaVStruct.totalDeltaV = 0;
            }

            if (deltaVChannel != null) deltaVChannel.Fire(OutboundPackets.DeltaV, myDeltaVStruct);
        }

        public void DeltaVEnvProvider()
        {
            DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();

            if (currentStageInfo != null)
            {

                myDeltaVEnvStruct.stageDeltaVASL = (float)currentStageInfo.deltaVatASL;
                myDeltaVEnvStruct.stageDeltaVVac = (float)currentStageInfo.deltaVinVac;
                myDeltaVEnvStruct.totalDeltaVASL = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalDeltaVASL;
                myDeltaVEnvStruct.totalDeltaVVac = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalDeltaVVac;
            }
            else
            {
                myDeltaVEnvStruct.stageDeltaVASL = 0;
                myDeltaVEnvStruct.stageDeltaVVac = 0;
                myDeltaVEnvStruct.totalDeltaVASL = 0;
                myDeltaVEnvStruct.totalDeltaVVac = 0;
            }

            if (deltaVEnvChannel != null) deltaVEnvChannel.Fire(OutboundPackets.DeltaVEnv, myDeltaVEnvStruct);
        }

        public void BurnTimeProvider()
        {
            DeltaVStageInfo currentStageInfo = getCurrentStageDeltaV();

            if (currentStageInfo != null)
            {
                myBurnTimeStruct.stageBurnTime = (float)currentStageInfo.stageBurnTime;
                myBurnTimeStruct.totalBurnTime = (float)FlightGlobals.ActiveVessel.VesselDeltaV.TotalBurnTime;
            }
            else
            {
                myBurnTimeStruct.stageBurnTime = 0;
                myBurnTimeStruct.totalBurnTime = 0;
            }

            if (burnTimeChannel != null) burnTimeChannel.Fire(OutboundPackets.BurnTime, myBurnTimeStruct);
        }
    }
}
