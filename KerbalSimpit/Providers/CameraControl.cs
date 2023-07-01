using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
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
            public short zoom;
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

        private float flightCameraPitchMultiplier = 0.00002f;
        private float flightCameraYawMultiplier = 0.00006f;
        private float flightCameraZoomMultiplier = 0.001f;

        private bool ivaCamFieldsLoaded = true;
        private FieldInfo ivaPitchField;
        private FieldInfo ivaYawField;
        private float ivaCameraMultiplier = 0.003f;


        public void Start()
        {

            LoadReflectionFields();
            receivedCameraControlMode = 0;
            oldCameraModeControl = 0;
            CameraModeChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.CameraMode);
            if (CameraModeChannel != null) CameraModeChannel.Add(cameraModeCallback);

            CameraRotationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.CameraRotation);
            if (CameraRotationChannel != null) CameraRotationChannel.Add(cameraRotationCallback);
            CameraTranslationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.CameraTranslation);
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


            // Switch based on the CameraControlBits values
            switch (controlMode)
            {
                // See if the control bit is a camera mode setting bit - need to add validity checking, to make sure the modes can be entered
                case CameraControlBits.FlightMode:
                    cameraManager.SetCameraFlight();
                    break;
                case CameraControlBits.IVAMode:
                    cameraManager.SetCameraIVA();
                    break;
                case CameraControlBits.PlanetaryMode:
                    PlanetariumCamera.fetch.Activate();
                    break;
                // If it is not a camera mode, it must be a mode for the (hopefully) current camera
                default:

                    // Based on the current camera mode, change where the CameraControl value gets sent
                    switch (cameraManager.currentCameraMode)
                    {
                        // Flight Camera
                        case CameraManager.CameraMode.Flight:
                            Debug.Log("Camera can have it's mode changed");
                            Debug.Log("Control mode: " + controlMode.ToString());
                            int currentFlightMode = FlightCamera.CamMode;
                            int maxEnum = Enum.GetNames(typeof(FlightCamera.Modes)).Length - 1;
                            Debug.Log("Max Enum Value: " + maxEnum.ToString());

                            // Switch based on the operation to perform on the flight camera
                            switch (controlMode)
                            {
                                case CameraControlBits.Auto:
                                    FlightCamera.SetMode(FlightCamera.Modes.AUTO);
                                    printCameraMode("Auto");
                                    break;
                                case CameraControlBits.Free:
                                    FlightCamera.SetMode(FlightCamera.Modes.FREE);
                                    printCameraMode("Free");
                                    break;
                                case CameraControlBits.Orbital:
                                    FlightCamera.SetMode(FlightCamera.Modes.ORBITAL);
                                    printCameraMode("Orbital");
                                    break;
                                case CameraControlBits.Chase:
                                    FlightCamera.SetMode(FlightCamera.Modes.CHASE);
                                    printCameraMode("Chase");
                                    break;
                                case CameraControlBits.Locked:
                                    FlightCamera.SetMode(FlightCamera.Modes.LOCKED);
                                    printCameraMode("Locked");
                                    break;
                                case CameraControlBits.NextCameraModeState:
                                    printCameraMode("Next");
                                    // If the current flight camera mode is at the end of the list, wrap around to the start
                                    if (currentFlightMode == maxEnum)
                                    {
                                        FlightCamera.SetMode((FlightCamera.Modes)0);
                                    }
                                    else
                                    {
                                        FlightCamera.SetMode((FlightCamera.Modes)currentFlightMode + 1);
                                    }
                                    break;
                                case CameraControlBits.PreviousCameraModeState:
                                    printCameraMode("Previous");
                                    // If the current flight camera mode is at the start of the list, wrap around to the end
                                    if (currentFlightMode == 0)
                                    {
                                        FlightCamera.SetMode((FlightCamera.Modes)maxEnum);
                                    }
                                    else
                                    {
                                        // Hack to get around the issue of orbital mode not being available below a certain altitude around SOIs.
                                        FlightCamera.Modes autoMode = FlightCamera.GetAutoModeForVessel(FlightGlobals.ActiveVessel);
                                        // If the Flight Camera mode tries to go backwards to orbital mode, and orbital mode is not available, set the camera mode back two places.
                                        if (((FlightCamera.Modes)(FlightCamera.CamMode - 1)) == FlightCamera.Modes.ORBITAL && autoMode != FlightCamera.Modes.ORBITAL)
                                        {
                                            FlightCamera.SetMode((FlightCamera.Modes)(currentFlightMode - 2));
                                        }
                                        // Otherwise just change the mode back by one
                                        else
                                        {
                                            FlightCamera.SetMode((FlightCamera.Modes)currentFlightMode - 1);
                                        }
                                    }
                                    break;
                                default:
                                    printCameraMode("No flight camera state to match the control bits:(");
                                    break;
                            }
                            break; /* End Flight Camera Mode */
                        case CameraManager.CameraMode.IVA:
                            break;

                        default:
                            Debug.Log(String.Format("Simpit control for the camera mode: {0} is unsupported", cameraManager.currentCameraMode.ToString()));
                            break;
                    } /* End of current camera mode switch */
                    break;
            } /* End of control mode switch */
        }

        private void printCameraMode(String cameraModeString)
        {
            if (KSPit.Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Set Camera Mode to: ", cameraModeString));
        }


        public void cameraRotationCallback(byte ID, object Data)
        {
            //Debug.Log("Camera Rotation Callback");
            newCameraRotation = KerbalSimpitUtils.ByteArrayToStructure<CameraRotationalStruct>((byte[])Data);
            // Bit fields:
            // pitch = 1
            // roll = 2
            // yaw = 4
            switch (cameraManager.currentCameraMode)
            {

                case CameraManager.CameraMode.Flight:
                    FlightCamera flightCamera = FlightCamera.fetch;
                    if ((newCameraRotation.mask & (byte)1) > 0)
                    {
                        myCameraRotation.pitch = newCameraRotation.pitch;
                        // Debug.Log("Rotation Message Seen");
                        float newPitch = flightCamera.camPitch + (myCameraRotation.pitch * flightCameraPitchMultiplier);
                        if (newPitch > flightCamera.maxPitch)
                        {
                            flightCamera.camPitch = flightCamera.maxPitch;
                        }
                        else if (newPitch < flightCamera.minPitch)
                        {
                            flightCamera.camPitch = flightCamera.minPitch;
                        }
                        else
                        {
                            flightCamera.camPitch = newPitch;
                        }

                    }
                    if ((newCameraRotation.mask & (byte)2) > 0)
                    {
                        myCameraRotation.roll = newCameraRotation.roll;
                    }
                    if ((newCameraRotation.mask & (byte)4) > 0)
                    {
                        myCameraRotation.yaw = newCameraRotation.yaw;
                        // Debug.Log("Yaw Message Seen");
                        float newHdg = flightCamera.camHdg + (myCameraRotation.yaw * flightCameraYawMultiplier);
                        flightCamera.camHdg = newHdg;
                    }
                    if ((newCameraRotation.mask & (byte)8) > 0)
                    {
                        myCameraRotation.zoom = newCameraRotation.zoom;
                        float newZoom = flightCamera.Distance + (myCameraRotation.zoom * flightCameraZoomMultiplier);
                        if(newZoom > flightCamera.maxDistance){
                            newZoom = flightCamera.maxDistance;
                        } else if (newZoom < flightCamera.minDistance){
                            newZoom = flightCamera.minDistance;
                        }
                        flightCamera.SetDistance(newZoom);
                    }
                    break;

                case CameraManager.CameraMode.IVA:
                case CameraManager.CameraMode.Internal:
                    Kerbal ivaKerbal = cameraManager.IVACameraActiveKerbal;

                    if (ivaKerbal == null)
                    {
                        Debug.Log("Kerbal is null");
                    }

                    InternalCamera ivaCamera = InternalCamera.Instance;
                    ivaCamera.mouseLocked = false;

                    if (ivaCamera == null)
                    {
                        Debug.Log("IVA Camera is null");
                    }
                    else
                    {
                        float newPitch = (float)ivaPitchField.GetValue(ivaCamera);
                        float newYaw = (float)ivaYawField.GetValue(ivaCamera);

                        if ((newCameraRotation.mask & (byte)1) > 0)
                        {
                            myCameraRotation.pitch = newCameraRotation.pitch;
                            //Debug.Log("IVA Rotation Message Seen");
                            newPitch += (myCameraRotation.pitch * ivaCameraMultiplier);

                            if (newPitch > ivaCamera.maxPitch)
                            {
                                newPitch = ivaCamera.maxPitch;
                            }
                            else if (newPitch < ivaCamera.minPitch)
                            {
                                newPitch = ivaCamera.minPitch;
                            }
                        }
                        if ((newCameraRotation.mask & (byte)2) > 0)
                        {
                            myCameraRotation.roll = newCameraRotation.roll;
                        }
                        if ((newCameraRotation.mask & (byte)4) > 0)
                        {
                            myCameraRotation.yaw = newCameraRotation.yaw;
                            //Debug.Log("IVA Yaw Message Seen");
                            newYaw += (myCameraRotation.yaw * ivaCameraMultiplier);
                            if (newYaw > 120f)
                            {
                                newYaw = 120f;
                            }
                            else if (newYaw < -120f)
                            {
                                newYaw = -120f;
                            }
                        }
                        //Debug.Log("Before set angle");
                        if (this.ivaCamFieldsLoaded)
                        {
                            ivaPitchField.SetValue(ivaCamera, newPitch);
                            ivaYawField.SetValue(ivaCamera, newYaw);
                            // Debug.Log("Camera vector: " + ivaCamera.transform.localEulerAngles.ToString());
                            FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
                        }
                    }
                    break;

                case CameraManager.CameraMode.Map:
                    //Debug.Log("Map Cam");
                    PlanetariumCamera planetariumCamera = PlanetariumCamera.fetch;
                    if ((newCameraRotation.mask & (byte)1) > 0)
                    {
                        myCameraRotation.pitch = newCameraRotation.pitch;
                        float newPitch = planetariumCamera.camPitch + (myCameraRotation.pitch * flightCameraPitchMultiplier);

                        newPitch = Mathf.Clamp(newPitch, planetariumCamera.minPitch, planetariumCamera.maxPitch);

                        planetariumCamera.camPitch = newPitch;

                    }
                    if ((newCameraRotation.mask & (byte)2) > 0)
                    {
                        myCameraRotation.roll = newCameraRotation.roll;
                    }
                    if ((newCameraRotation.mask & (byte)4) > 0)
                    {
                        myCameraRotation.yaw = newCameraRotation.yaw;
                        float newHdg = planetariumCamera.camHdg + (myCameraRotation.yaw * flightCameraYawMultiplier);
                        planetariumCamera.camHdg = newHdg;
                    }
                    if ((newCameraRotation.mask & (byte)8) > 0)
                    {
                        myCameraRotation.zoom = newCameraRotation.zoom;
                        float newZoom = planetariumCamera.Distance + (myCameraRotation.zoom * flightCameraZoomMultiplier);

                        newZoom = Mathf.Clamp(newZoom, planetariumCamera.minDistance, planetariumCamera.maxDistance);

                        planetariumCamera.SetDistance(newZoom);
                    }
                    break;
                default:
                    Debug.Log("Kerbal Simpit does not support this camera mode: " + cameraManager.currentCameraMode.ToString());
                    break;
            }
        }

        private void LoadReflectionFields()
        {
            List<FieldInfo> fields = new List<FieldInfo>(typeof(InternalCamera).GetFields(
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            fields = new List<FieldInfo>(fields.Where(f => f.FieldType.Equals(typeof(float))));
            this.ivaPitchField = fields[3];
            this.ivaYawField = fields[4];
            if (ivaPitchField == null || ivaYawField == null)
            {
                this.ivaCamFieldsLoaded = false;
                Debug.LogWarning("AFBW - Failed to acquire pitch/yaw fields in InternalCamera");
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
