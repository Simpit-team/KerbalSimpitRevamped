using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimPitTelemetryProvider : MonoBehaviour
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
        public struct VelocityStruct
        {
            public float orbital;
            public float surface;
            public float vertical;
        }

        private AltitudeStruct myAlt;
        private ApsidesStruct myApsides;
        private VelocityStruct myVelocity;

        private EventData<byte, object> altitudeChannel, apsidesChannel,
            velocityChannel;

        public void Start()
        {
            KSPit.AddToDeviceHandler(AltitudeProvider);
            altitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial8");
            KSPit.AddToDeviceHandler(ApsidesProvider);
            apsidesChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial9");
            KSPit.AddToDeviceHandler(VelocityProvider);
            velocityChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial22");
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(AltitudeProvider);
            KSPit.RemoveToDeviceHandler(ApsidesProvider);
            KSPit.RemoveToDeviceHandler(VelocityProvider);
        }

        public void AltitudeProvider()
        {
            myAlt.alt = (float)FlightGlobals.ActiveVessel.altitude;
            myAlt.surfalt = (float)FlightGlobals.ActiveVessel.radarAltitude;
            if (altitudeChannel != null) altitudeChannel.Fire(OutboundPackets.Altitude, myAlt);
        }

        public void ApsidesProvider()
        {
            Orbit curOrbit = FlightGlobals.ActiveVessel.orbit;
            myApsides.apoapsis = (float)curOrbit.ApA;
            myApsides.periapsis = (float)curOrbit.PeA;
            if (apsidesChannel != null) apsidesChannel.Fire(OutboundPackets.Apsides, myApsides);
        }

        public void VelocityProvider()
        {
            Orbit curOrbit = FlightGlobals.ActiveVessel.orbit;
            myVelocity.orbital = (float)FlightGlobals.ActiveVessel.obt_speed;
            myVelocity.surface = (float)FlightGlobals.ActiveVessel.srfSpeed;
            myVelocity.vertical = (float)FlightGlobals.ActiveVessel.verticalSpeed;
            if (velocityChannel != null) velocityChannel.Fire(OutboundPackets.Velocities, myVelocity);
        }
    }
}
