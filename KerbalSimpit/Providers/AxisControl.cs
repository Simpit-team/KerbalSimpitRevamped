using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitAxisController : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct ThreeAxisStruct
        {
            public short pitch;
            public short roll;
            public short yaw;
            public byte mask;
        }
        // Inbound messages
        private EventData<byte, object> VesselAttitudeChannel;

        private ThreeAxisStruct myControls;
        private volatile bool myControlsFlag;

        public void Start()
        {
            VesselAttitudeChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived16");
            if (VesselAttitudeChannel != null) VesselAttitudeChannel.Add(vesselAttitudeCallback);
        }

        public void OnDestroy()
        {
            if (VesselAttitudeChannel != null) VesselAttitudeChannel.Add(vesselAttitudeCallback);
        }

        public void vesselAttitudeCallback(byte ID, object Data)
        {
            // Nothing yet.
        }
    }
}

