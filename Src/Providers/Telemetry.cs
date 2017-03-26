using System;
using System.Runtime.InteropServices;
using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitTelemetryProvider : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
    public struct AltitudeStruct
    {
        public float alt;
        public float surfalt;
    }

    private AltitudeStruct myAlt;

    private EventData<byte, object> altitudeChannel;

    public void Start()
    {
        KerbalSimPit.AddToDeviceHandler(AltitudeProvider);
        altitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial4");
    }

    public void OnDestroy()
    {
        if (KerbalSimPit.RemoveToDeviceHandler(AltitudeProvider))
        {
            if (KerbalSimPit.Config.Verbose)
            {
                Debug.Log("KerbalSimPit: Succesfully removed AltitudeProvider");
            } else {
                Debug.Log("KerbalSimPit: Couldn't remove AltitudeProvider");
            }
        }
    }

    public void AltitudeProvider()
    {
        myAlt.alt = (float)FlightGlobals.ActiveVessel.altitude;
        myAlt.surfalt = (float)FlightGlobals.ActiveVessel.radarAltitude;
        if (altitudeChannel != null) altitudeChannel.Fire(0x04, myAlt);
    }
}
