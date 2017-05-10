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

        private AltitudeStruct myAlt;
        private ApsidesStruct myApsides;
        private EventData<byte, object> altitudeChannel, apsidesChannel;

        public void Start()
        {
            KSPit.AddToDeviceHandler(AltitudeProvider);
            altitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial8");
            KSPit.AddToDeviceHandler(ApsidesProvider);
            apsidesChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial9");
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(AltitudeProvider);
            KSPit.RemoveToDeviceHandler(ApsidesProvider);
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
    }
}
