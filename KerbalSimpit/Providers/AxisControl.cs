using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimpit;
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct SASModeInfoStruct
        {
            public byte currentSASMode;
            public ushort SASModeAvailability;
        }

        // Inbound messages
        private EventData<byte, object> RotationChannel, TranslationChannel,
            WheelChannel, ThrottleChannel, SASInfoChannel, AutopilotChannel;

        private RotationalStruct myRotation, newRotation;

        private TranslationalStruct myTranslation, newTranslation;

        private WheelStruct myWheel, newWheel;

        private SASModeInfoStruct mySASInfo, newSASInfo;

        private short myThrottle;
        private volatile bool myThrottleFlag;

        private VesselAutopilot.AutopilotMode mySASMode;
        private Vessel lastActiveVessel;

        public void Start()
        {
            RotationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived16");
            if (RotationChannel != null) RotationChannel.Add(vesselRotationCallback);
            TranslationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived17");
            if (TranslationChannel != null) TranslationChannel.Add(vesselTranslationCallback);
            WheelChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived18");
            if (WheelChannel != null) WheelChannel.Add(wheelCallback);
            ThrottleChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived19");
            if (ThrottleChannel != null) ThrottleChannel.Add(throttleCallback);
            AutopilotChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived20");
            if (AutopilotChannel != null) AutopilotChannel.Add(autopilotModeCallback);

            SASInfoChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.SASInfo);
            KSPit.AddToDeviceHandler(SASInfoProvider);
            mySASInfo.currentSASMode = 255; // value for not enabled
            mySASInfo.SASModeAvailability = 0;

            lastActiveVessel = FlightGlobals.ActiveVessel;
            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
            GameEvents.onVesselChange.Add(OnVesselChange);
        }

        public void OnDestroy()
        {
            if (RotationChannel != null) RotationChannel.Remove(vesselRotationCallback);
            if (TranslationChannel != null) TranslationChannel.Remove(vesselTranslationCallback);
            if (WheelChannel != null) WheelChannel.Remove(wheelCallback);
            if (ThrottleChannel!= null) ThrottleChannel.Remove(throttleCallback);
            if (AutopilotChannel!= null) AutopilotChannel.Remove(autopilotModeCallback);

            KSPit.RemoveToDeviceHandler(SASInfoProvider);

            lastActiveVessel.OnPostAutopilotUpdate -= AutopilotUpdater;
            GameEvents.onVesselChange.Remove(OnVesselChange);
        }

        public void OnVesselChange(Vessel vessel)
        {
            lastActiveVessel.OnPostAutopilotUpdate -= AutopilotUpdater;
            lastActiveVessel = FlightGlobals.ActiveVessel;
            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
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
            myThrottle = BitConverter.ToInt16((byte[])Data, 0);
            myThrottleFlag = true;
        }

        public void autopilotModeCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;

            VesselAutopilot autopilot = FlightGlobals.ActiveVessel.Autopilot;

            if(autopilot == null)
            {
                Debug.Log("KerbalSimpit : Ignoring a SAS MODE Message since I could not find the autopilot");
            }

            mySASMode = (VesselAutopilot.AutopilotMode)(payload[0]);

            if (autopilot.CanSetMode(mySASMode))
            {
                autopilot.SetMode(mySASMode);
                if (KSPit.Config.Verbose)
                {
                    Debug.Log(String.Format("KerbalSimpit: payload is {0}", mySASMode));
                    Debug.Log(String.Format("KerbalSimpit: SAS mode is {0}", FlightGlobals.ActiveVessel.Autopilot.Mode.ToString()));
                }
            }
            else
            {
                Debug.Log(String.Format("KerbalSimpit: Unable to set SAS mode to {0}", mySASMode.ToString()));
            }
        }

        public void AutopilotUpdater(FlightCtrlState fcs)
        {
            if (myRotation.pitch != 0)
            {
                fcs.pitch = (float)myRotation.pitch/ Int16.MaxValue;
            }
            if (myRotation.roll != 0)
            {
                fcs.roll = (float)myRotation.roll/ Int16.MaxValue;
            }
            if (myRotation.yaw != 0)
            {
                fcs.yaw = (float)myRotation.yaw/ Int16.MaxValue;
            }

            if (myTranslation.X != 0)
            {
                fcs.X = (float)myTranslation.X/ Int16.MaxValue;
            }
            if (myTranslation.Y != 0)
            {
                fcs.Y = (float)myTranslation.Y/ Int16.MaxValue;
            }
            if (myTranslation.Z != 0)
            {
                fcs.Z = (float)myTranslation.Z/ Int16.MaxValue;
            }

            if (myWheel.steer != 0)
            {
                fcs.wheelSteer = (float)myWheel.steer/ Int16.MaxValue;
            }
            if (myWheel.throttle != 0)
            {
                fcs.wheelThrottle = (float)myWheel.throttle/ Int16.MaxValue;
            }

            if (myThrottleFlag)
            {
                fcs.mainThrottle = (float)myThrottle/ Int16.MaxValue;
            }
        }

        public void SASInfoProvider()
        {
            VesselAutopilot autopilot = FlightGlobals.ActiveVessel.Autopilot;

            if(autopilot == null)
            {
                return;
            }

            if (autopilot.Enabled)
            {
                newSASInfo.currentSASMode = (byte)autopilot.Mode;
            } else
            {
                newSASInfo.currentSASMode = 255; //special value to indicate a disabled SAS
            }

            newSASInfo.SASModeAvailability = 0;
            foreach (VesselAutopilot.AutopilotMode i in Enum.GetValues(typeof(VesselAutopilot.AutopilotMode)))
            {
                if (autopilot.CanSetMode(i))
                {
                    newSASInfo.SASModeAvailability = (ushort) (newSASInfo.SASModeAvailability | (1 << (byte)i));
                }
            }

            if(mySASInfo.currentSASMode != newSASInfo.currentSASMode ||
                mySASInfo.SASModeAvailability != newSASInfo.SASModeAvailability)
            {
                if (SASInfoChannel != null)
                {
                    mySASInfo = newSASInfo;
                    Debug.Log("I'm in mode " + mySASInfo.currentSASMode + " with availability " + Convert.ToString(mySASInfo.SASModeAvailability, 2));
                    SASInfoChannel.Fire(OutboundPackets.SASInfo, mySASInfo);
                }
            }
        }
    }
}
