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

        private AltitudeStruct myAlt;
        private ApsidesStruct myApsides;
        private ApsidesTimeStruct myApsidesTime;
        private VelocityStruct myVelocity;
        private AirspeedStruct myAirspeed;

        private EventData<byte, object> altitudeChannel, apsidesChannel,
            apsidesTimeChannel, velocityChannel, soiChannel, airspeedChannel;

        private string CurrentSoI;

        public void Start()
        {
            KSPit.AddToDeviceHandler(AltitudeProvider);
            altitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial8");
            KSPit.AddToDeviceHandler(ApsidesProvider);
            apsidesChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial9");
            KSPit.AddToDeviceHandler(ApsidesTimeProvider);
            apsidesTimeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial24");
            KSPit.AddToDeviceHandler(VelocityProvider);
            velocityChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial22");
            // We fire one SoI packet when SoI changes. So no need to use the
            // periodic DeviceHandler infrastructure.
            CurrentSoI = "";
            soiChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial26");
            KSPit.AddToDeviceHandler(AirspeedProvider);
            airspeedChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial27");
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(AltitudeProvider);
            KSPit.RemoveToDeviceHandler(ApsidesProvider);
            KSPit.RemoveToDeviceHandler(ApsidesTimeProvider);
            KSPit.RemoveToDeviceHandler(VelocityProvider);
            KSPit.RemoveToDeviceHandler(AirspeedProvider);
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
    }
}
