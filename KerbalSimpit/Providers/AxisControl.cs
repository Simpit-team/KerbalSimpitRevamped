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
            if (RotationChannel != null) RotationChannel.Add(vesselRotationCallback);
            TranslationChannel = GameEvents.FindEvent<EventData<byte, object>>("OnSerialReceived17");
            if (TranslationChannel != null) TranslationChannel.Add(vesselTranslationCallback);
            WheelChannel = GameEvents.FindEvent<EventData<byte, object>>("OnSerialReceived18");
            if (WheelChannel != null) WheelChannel.Add(wheelCallback);

            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
        }

        public void OnDestroy()
        {
            if (RotationChannel != null) RotationChannel.Remove(vesselRotationCallback);
            if (TranslationChannel != null) TranslationChannel.Remove(vesselTranslationCallback);
            if (WheelChannel != null) WheelChannel.Remove(wheelCallback);

            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate -= AutopilotUpdater;
        }

        public void vesselRotationCallback(byte ID, object Data)
        {
            myRotation = KerbalSimpitUtils.ByteArrayToStructure<RotationalStruct>((byte[])Data);
            myRotationFlag = true;
        }

        public void vesselTranslationCallback(byte ID, object Data)
        {
            myTranslation = KerbalSimpitUtils.ByteArrayToStructure<TranslationalStruct>((byte[])Data);
            myTranslationFlag = true;
        }

        public void wheelCallback(byte ID, object Data)
        {
            myWheel = KerbalSimpitUtils.ByteArrayToStructure<WheelStruct>((byte[])Data);
            myWheelFlag = true;
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
            if (myTranslationFlag)
            {
                // Again, future Peter: Bit fields:
                // X = 1
                // Y = 2
                // Z = 4
                if ((myTranslation.mask & (byte)1) > 0)
                {
                    fcs.X = myTranslation.X;
                }
                if ((myTranslation.mask & (byte)2) > 0)
                {
                    fcs.Y = myTranslation.Y;
                }
                if ((myTranslation.mask & (byte)4) > 0)
                {
                    fcs.Z = myTranslation.Z;
                }
                myTranslationFlag = false;
            }
            if (myWheelFlag)
            {
                if ((myWheel.mask & (byte)1) > 0)
                {
                    fcs.wheelSteer = myWheel.steer;
                }
                if ((myWheel.mask & (byte)2) > 0)
                {
                    fcs.wheelThrottle = myWheel.throttle;
                }
                myWheelFlag = false;
            }
        }
    }
}

