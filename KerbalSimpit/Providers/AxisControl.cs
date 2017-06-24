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

        private RotationalStruct myRotation, newRotation;

        private TranslationalStruct myTranslation, newTranslation;

        private WheelStruct myWheel, newWheel;

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
            ThrottleChannel = GameEvents.FindEvent<EventData<byte, object>>("OnSerialReceived19");
            if (ThrottleChannel != null) ThrottleChannel.Add(throttleCallback);

            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
        }

        public void OnDestroy()
        {
            if (RotationChannel != null) RotationChannel.Remove(vesselRotationCallback);
            if (TranslationChannel != null) TranslationChannel.Remove(vesselTranslationCallback);
            if (WheelChannel != null) WheelChannel.Remove(wheelCallback);
            if (ThrottleChannel!= null) ThrottleChannel.Remove(throttleCallback);

            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate -= AutopilotUpdater;
        }

        public void vesselRotationCallback(byte ID, object Data)
        {
            
            newRotation = KerbalSimpitUtils.ByteArrayToStructure<RotationalStruct>((byte[])Data);
            // Bit fields:
            // pitch = 1
            // roll = 2
            // yaw = 4
            if ((newRotation.mask & (byte)1) > 0)
            {
                myRotation.pitch = newRotation.pitch;
            }
            if ((newRotation.mask & (byte)2) > 0)
            {
                myRotation.roll = newRotation.roll;
            }
            if ((newRotation.mask & (byte)4) > 0)
            {
                myRotation.yaw = newRotation.yaw;
            }
        }

        public void vesselTranslationCallback(byte ID, object Data)
        {
            newTranslation = KerbalSimpitUtils.ByteArrayToStructure<TranslationalStruct>((byte[])Data);
            // Bit fields:
            // X = 1
            // Y = 2
            // Z = 4
            if ((newTranslation.mask & (byte)1) > 0)
            {
                myTranslation.X = newTranslation.X;
            }
            if ((newTranslation.mask & (byte)2) > 0)
            {
                myTranslation.Y = newTranslation.Y;
            }
            if ((newTranslation.mask & (byte)4) > 0)
            {
                myTranslation.Z = newTranslation.Z;
            }
        }

        public void wheelCallback(byte ID, object Data)
        {
            newWheel = KerbalSimpitUtils.ByteArrayToStructure<WheelStruct>((byte[])Data);
            // Bit fields
            // steer = 1
            // throttle = 2
            if ((newWheel.mask & (byte)1) > 0)
            {
                myWheel.steer = newWheel.steer;
            }
            if ((newWheel.mask & (byte)2) > 0)
            {
                myWheel.throttle = newWheel.throttle;
            }
        }

        public void throttleCallback(byte ID, object Data)
        {
            // TODO: check length and the like here. Also everywhere else?
            // Actually let me add that to my Trello board.
            myThrottle = (short)Data;
            myThrottleFlag = true;
        }

        public void AutopilotUpdater(FlightCtrlState fcs)
        {
            if (myRotation.pitch != 0)
            {
                fcs.pitch = (float)myRotation.pitch/32767;
            }
            if (myRotation.roll != 0)
            {
                fcs.roll = (float)myRotation.roll/32767;
            }
            if (myRotation.yaw != 0)
            {
                fcs.yaw = (float)myRotation.yaw/32767;
            }

            if (myTranslation.X != 0)
            {
                fcs.X = (float)myTranslation.X/32767;
            }
            if (myTranslation.Y != 0)
            {
                fcs.Y = (float)myTranslation.Y/32767;
            }
            if (myTranslation.Z != 0)
            {
                fcs.Z = (float)myTranslation.Z/32767;
            }

            if (myWheel.steer != 0)
            {
                fcs.wheelSteer = (float)myWheel.steer/32767;
            }
            if (myWheel.throttle != 0)
            {
                fcs.wheelThrottle = (float)myWheel.throttle/32767;
            }

            if (myThrottleFlag)
            {
                fcs.mainThrottle = (float)myThrottle/32767;
            }
        }
    }
}

