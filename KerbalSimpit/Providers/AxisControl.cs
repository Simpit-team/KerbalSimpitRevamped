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
        public struct RotationalStruct
        {
            public short pitch;
            public short roll;
            public short yaw;
            public byte mask;
        }
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct TranslationalStruct
        {
            public short X;
            public short Y;
            public short Z;
            public byte mask;
        }
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct WheelStruct
        {
            public short steer;
            public short throttle;
            public byte mask;
        }

        // Inbound messages
        private EventData<byte, object> RotationChannel, TranslationChannel,
            WheelChannel, ThrottleChannel;

        private RotationalStruct myRotation;
        private volatile bool myRotationFlag;

        private TranslationalStruct myTranslation;
        private volatile bool myTranslationFlag;

        private WheelStruct myWheel;
        private volatile bool myWheelFlag;

        private short myThrottle;
        private volatile bool myThrottleFlag;
        
        public void Start()
        {
            RotationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived16");
            if (RotationChannel != null) RotationChannel.Add(vesselAttitudeCallback);
            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
        }

        public void OnDestroy()
        {
            if (RotationChannel != null) RotationChannel.Add(vesselAttitudeCallback);
            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate -= AutopilotUpdater;
        }

        public void vesselAttitudeCallback(byte ID, object Data)
        {
            myRotation = KerbalSimpitUtils.ByteArrayToStructure<RotationalStruct>((byte[])Data);
            myRotationFlag = true;
        }

        public void AutopilotUpdater(FlightCtrlState fcs)
        {
            if (myRotationFlag)
            {
                // Note to future Peter: You documented the bit fields here:
                // pitch = 1
                // roll = 2
                // yaw = 4
                if ((myRotation.mask & (byte)1) > 0)
                {
                    fcs.pitch = myRotation.pitch;
                }
                if ((myRotation.mask & (byte)2) > 0)
                {
                    fcs.roll = myRotation.roll;
                }
                if ((myRotation.mask & (byte)4) > 0)
                {
                    fcs.yaw = myRotation.yaw;
                }
                myRotationFlag = false;
            }
        }
    }
}

