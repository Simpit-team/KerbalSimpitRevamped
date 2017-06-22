using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimpit.Utilities;

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
            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
        }

        public void OnDestroy()
        {
            if (VesselAttitudeChannel != null) VesselAttitudeChannel.Add(vesselAttitudeCallback);
            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate -= AutopilotUpdater;
        }

        public void vesselAttitudeCallback(byte ID, object Data)
        {
            myControls = KerbalSimpitUtils.ByteArrayToStructure<ThreeAxisStruct>((byte[])Data);
            myControlsFlag = true;
        }

        public void AutopilotUpdater(FlightCtrlState fcs)
        {
            if (myControlsFlag)
            {
                // Note to future Peter: You documented the bit fields here:
                // pitch = 1
                // roll = 2
                // yaw = 4
                if ((myControls.mask & (byte)1) > 0)
                {
                    fcs.pitch = myControls.pitch;
                }
                if ((myControls.mask & (byte)2) > 0)
                {
                    fcs.roll = myControls.roll;
                }
                if ((myControls.mask & (byte)4) > 0)
                {
                    fcs.yaw = myControls.yaw;
                }
                myControlsFlag = false;
            }
        }
    }
}

