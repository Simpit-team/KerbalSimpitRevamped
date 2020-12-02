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

        // Inbound messages
        private EventData<byte, object> RotationChannel, TranslationChannel,
            WheelChannel, ThrottleChannel, AutopilotChannel;

        // Outbound messages

        private EventData<byte, object> SASModeChannel;

        private RotationalStruct myRotation, newRotation;

        private TranslationalStruct myTranslation, newTranslation;

        private WheelStruct myWheel, newWheel;

        private short myThrottle;
        private volatile bool myThrottleFlag;

        private VesselAutopilot.AutopilotMode mySASMode;

        private VesselAutopilot.AutopilotMode prevSASMode;
        private VesselAutopilot.AutopilotMode currentSASMode;

        private Vessel myActiveVessel;

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

            SASModeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial36");

            FlightGlobals.ActiveVessel.OnPostAutopilotUpdate += AutopilotUpdater;
        }

        public void Update()
        {
            currentSASMode = FlightGlobals.ActiveVessel.Autopilot.Mode;

            if (currentSASMode != prevSASMode)
            {
                byte SASModeSend=0;

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.Prograde)){
                    SASModeSend = 1;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Prograde"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.Retrograde)){
                    SASModeSend = 2;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Retrograde"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.Normal)){
                    SASModeSend = 3;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Normal"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.Antinormal)){
                    SASModeSend = 4;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Anti-Normal"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.RadialIn)){
                    SASModeSend = 5;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Radial In"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.RadialOut)){
                    SASModeSend = 6;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Radial Out"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.Target)){
                    SASModeSend = 7;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Target"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.AntiTarget)){
                    SASModeSend = 8;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Anti-Target"));
                }

                if (currentSASMode.Equals(VesselAutopilot.AutopilotMode.Maneuver)){
                    SASModeSend = 9;
                    Debug.Log(String.Format("KerbalSimpit: SAS Mode is Maneuver"));
                }

                if (SASModeChannel != null)
                {
                    SASModeChannel.Fire(OutboundPackets.SASMode, SASModeSend);
                }
            }
            prevSASMode = currentSASMode;
        }

        public void OnDestroy()
        {
            if (RotationChannel != null) RotationChannel.Remove(vesselRotationCallback);
            if (TranslationChannel != null) TranslationChannel.Remove(vesselTranslationCallback);
            if (WheelChannel != null) WheelChannel.Remove(wheelCallback);
            if (ThrottleChannel!= null) ThrottleChannel.Remove(throttleCallback);
            if (AutopilotChannel!= null) AutopilotChannel.Remove(autopilotModeCallback);

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
            myThrottle = BitConverter.ToInt16((byte[])Data, 0);
            myThrottleFlag = true;
        }

        public void autopilotModeCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
	    mySASMode = (VesselAutopilot.AutopilotMode)(payload[0]);
	    myActiveVessel = FlightGlobals.ActiveVessel;

	    if (myActiveVessel.Autopilot.CanSetMode(mySASMode)) {
		myActiveVessel.Autopilot.SetMode((VesselAutopilot.AutopilotMode)mySASMode);
		if(KSPit.Config.Verbose)
		{
		    Debug.Log(String.Format("KerbalSimpit: payload is {0}", mySASMode));
		    Debug.Log(String.Format("KerbalSimpit: SAS mode is {0}", myActiveVessel.Autopilot.Mode.ToString()));
		}
	    } else {
		Debug.Log(String.Format("KerbalSimpit: Unable to set SAS mode to {0}", mySASMode.ToString()));
	    }
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
            		if(KSPit.Config.Verbose)
                {
            		    Debug.Log(String.Format("KerbalSimpit: Setting throttle to {0}/32767", myThrottle));
            		}
                fcs.mainThrottle = (float)myThrottle/32767;
            }
        }
    }
}
