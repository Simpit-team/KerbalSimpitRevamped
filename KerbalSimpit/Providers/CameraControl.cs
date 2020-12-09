using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimpit;
using KerbalSimpit.Utilities;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitCameraControl : MonoBehaviour
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct CameraModeStruct
        {
            public byte cameraMode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct CameraRotationalStruct
        {
            public short pitch;
            public short roll;
            public short yaw;
            public byte mask;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct CameraTranslationalStruct
        {
            public short X;
            public short Y;
            public short Z;
            public byte mask;
        }

        // Inbound messages
        private EventData<byte, object> CameraModeChannel;

        private EventData<byte, object> CameraRotationChannel, CameraTranslationChannel;

        private volatile byte receivedCameraControlMode, oldCameraModeControl;
        private volatile Boolean needToUpdateCamera = false;

        private CameraRotationalStruct myCameraRotation, newCameraRotation;

        private CameraTranslationalStruct myCameraTranslation, newCameraTranslation;

        private CameraManager cameraManager = CameraManager.Instance;

        public void Start()
        {

            receivedCameraControlMode = 0;
            oldCameraModeControl = 0;
            CameraModeChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived21");
            if (CameraModeChannel != null) CameraModeChannel.Add(cameraModeCallback);

            CameraRotationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived22");
            if (CameraRotationChannel != null) CameraRotationChannel.Add(cameraRotationCallback);
            CameraTranslationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived23");
            if (CameraTranslationChannel != null) CameraTranslationChannel.Add(cameraTranslationCallback);

        }

        public void OnDestroy()
        {
            if (CameraModeChannel != null) CameraModeChannel.Remove(cameraModeCallback);
            if (CameraRotationChannel != null) CameraRotationChannel.Remove(cameraRotationCallback);
            if (CameraTranslationChannel != null) CameraTranslationChannel.Remove(cameraTranslationCallback);

        }

        public void Update()
        {
            if (needToUpdateCamera)
            {
                updateCameraMode(receivedCameraControlMode);
            }
        }


        public void cameraModeCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            receivedCameraControlMode = payload[0];
            needToUpdateCamera = true;
        }

        private void updateCameraMode(byte controlMode)
        {
            Debug.Log("Camera update called");
            oldCameraModeControl = receivedCameraControlMode;
            needToUpdateCamera = false;

            if (controlMode < 10 && cameraManager.currentCameraMode == CameraManager.CameraMode.Flight)
            {
                Debug.Log("Camera can have it's mode changed");
                Debug.Log("Control mode: " + controlMode.ToString());
                int currentFlightMode = FlightCamera.CamMode;
                int maxEnum = Enum.GetNames(typeof(FlightCamera.Modes)).Length - 1;
                Debug.Log("Max Enum Value: " + maxEnum.ToString());
                int caseSwitch = controlMode;

                switch (caseSwitch)
                {
                    case CameraControlBits.Auto:
                        FlightCamera.SetMode(FlightCamera.Modes.AUTO);
                        Debug.Log("Camera Mode: Auto");
                        break;
                    case CameraControlBits.Free:
                        FlightCamera.SetMode(FlightCamera.Modes.FREE);
                        Debug.Log("Camera Mode: Free");
                        break;
                    case CameraControlBits.Orbital:
                        FlightCamera.SetMode(FlightCamera.Modes.ORBITAL);
                        Debug.Log("Camera Mode: Orbital");
                        break;
                    case CameraControlBits.Chase:
                        FlightCamera.SetMode(FlightCamera.Modes.CHASE);
                        Debug.Log("Camera Mode: Chase");
                        break;
                    case CameraControlBits.Locked:
                        FlightCamera.SetMode(FlightCamera.Modes.LOCKED);
                        Debug.Log("Camera Mode: Locked");
                        break;
                    case CameraControlBits.NextMode:
                        Debug.Log("Camera Mode: Next");
                        if (currentFlightMode == maxEnum)
                        {
                            FlightCamera.SetMode((FlightCamera.Modes)0);
                        }
                        else
                        {
                            FlightCamera.SetMode((FlightCamera.Modes)currentFlightMode + 1);
                        }
                        break;
                    case CameraControlBits.PreviousMode:
                        Debug.Log("Camera Mode: Previous");
                        if (currentFlightMode == 0)
                        {
                            FlightCamera.SetMode((FlightCamera.Modes)maxEnum);
                        }
                        else
                        {
                            FlightCamera.Modes autoMode = FlightCamera.GetAutoModeForVessel(FlightGlobals.ActiveVessel);
                            if (((FlightCamera.Modes)(FlightCamera.CamMode - 1)) == FlightCamera.Modes.ORBITAL && autoMode != FlightCamera.Modes.ORBITAL)
                            {
                                FlightCamera.SetMode((FlightCamera.Modes)(currentFlightMode - 2));
                                FlightCamera.Modes temp = (FlightCamera.Modes)(currentFlightMode - 2);
                                Debug.Log("Camera should be: " + temp.ToString());
                            } else {
                                FlightCamera.SetMode((FlightCamera.Modes)currentFlightMode - 1);
                            }
                        }
                        break;
                    default:
                        Debug.Log("Camera Mode: No Camera Mode :(");
                        break;
                }
            }
        }


        public void cameraRotationCallback(byte ID, object Data)
        {

            newCameraRotation = KerbalSimpitUtils.ByteArrayToStructure<CameraRotationalStruct>((byte[])Data);
            // Bit fields:
            // pitch = 1
            // roll = 2
            // yaw = 4
            if ((newCameraRotation.mask & (byte)1) > 0)
            {
                myCameraRotation.pitch = newCameraRotation.pitch;
            }
            if ((newCameraRotation.mask & (byte)2) > 0)
            {
                myCameraRotation.roll = newCameraRotation.roll;
            }
            if ((newCameraRotation.mask & (byte)4) > 0)
            {
                myCameraRotation.yaw = newCameraRotation.yaw;
            }
        }

        public void cameraTranslationCallback(byte ID, object Data)
        {
            newCameraTranslation = KerbalSimpitUtils.ByteArrayToStructure<CameraTranslationalStruct>((byte[])Data);
            // Bit fields:
            // X = 1
            // Y = 2
            // Z = 4
            if ((newCameraTranslation.mask & (byte)1) > 0)
            {
                myCameraTranslation.X = newCameraTranslation.X;
            }
            if ((newCameraTranslation.mask & (byte)2) > 0)
            {
                myCameraTranslation.Y = newCameraTranslation.Y;
            }
            if ((newCameraTranslation.mask & (byte)4) > 0)
            {
                myCameraTranslation.Z = newCameraTranslation.Z;
            }
        }
    }
}
