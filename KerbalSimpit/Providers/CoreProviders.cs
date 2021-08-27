using System;
using System.Linq;
using System.Runtime.InteropServices;
using KSP.IO;
using UnityEngine;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitEchoProvider : MonoBehaviour
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct FlightStatusStruct
        {
            public byte flightStatusFlags; // content defined with the FlightStatusBits
            public byte vesselSituation; // See Vessel.Situations for possible values
            public byte currentTWIndex;
            public byte crewCapacity;
            public byte crewCount;
            public byte commNetSignalStrenghPercentage;
        }

        private FlightStatusStruct myFlightStatus;

        private EventData<byte, object> echoRequestEvent;
        private EventData<byte, object> echoReplyEvent;
        private EventData<byte, object> customLogEvent;
        private EventData<byte, object> sceneChangeEvent;
        private EventData<byte, object> flightStatusChannel;

        public void Start()
        {
            echoRequestEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + CommonPackets.EchoRequest);
            if (echoRequestEvent != null) echoRequestEvent.Add(EchoRequestCallback);
            echoReplyEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + CommonPackets.EchoResponse);
            if (echoReplyEvent != null) echoReplyEvent.Add(EchoReplyCallback);
            customLogEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.CustomLog);
            if (customLogEvent != null) customLogEvent.Add(CustomLogCallback);

            KSPit.AddToDeviceHandler(FlightStatusProvider);
            flightStatusChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.FlightStatus);

            sceneChangeEvent = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.SceneChange);

            GameEvents.onFlightReady.Add(FlightReadyHandler);
            GameEvents.onGameSceneSwitchRequested.Add(FlightShutdownHandler);
        }

        public void OnDestroy()
        {
            if (echoRequestEvent != null) echoRequestEvent.Remove(EchoRequestCallback);
            if (echoReplyEvent != null) echoReplyEvent.Remove(EchoReplyCallback);
            if (customLogEvent != null) customLogEvent.Remove(CustomLogCallback);

            KSPit.RemoveToDeviceHandler(FlightStatusProvider);

            GameEvents.onFlightReady.Remove(FlightReadyHandler);
            GameEvents.onGameSceneSwitchRequested.Remove(FlightShutdownHandler);
        }

        public void EchoRequestCallback(byte ID, object Data)
        {
            if (KSPit.Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Echo request on port {0}. Replying.", ID));
            KSPit.SendToSerialPort(ID, CommonPackets.EchoResponse, Data);
        }

        public void EchoReplyCallback(byte ID, object Data)
        {
            Debug.Log(String.Format("KerbalSimpit: Echo reply received on port {0}.", ID));
        }

        public void CustomLogCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;

            byte logStatus = payload[0];
            String message = System.Text.Encoding.UTF8.GetString(payload.Skip(1).ToArray());

            if((logStatus & CustomLogBits.NoHeader) == 0)
            {
                message = "Simpit : " + message;
            }

            if ((logStatus & CustomLogBits.PrintToScreen) != 0)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => ScreenMessages.PostScreenMessage(message));
            }

            if ((logStatus & CustomLogBits.Verbose) == 0 || KSPit.Config.Verbose)
            {
                Debug.Log(message);
            }
        }

        private void FlightReadyHandler()
        {
            if (sceneChangeEvent != null)
            {
                sceneChangeEvent.Fire(OutboundPackets.SceneChange, 0x00);
            }
        }

        private void FlightShutdownHandler(GameEvents.FromToAction
                                           <GameScenes, GameScenes> scenes)
        {
            if (scenes.from == GameScenes.FLIGHT)
            {
                if (sceneChangeEvent != null)
                {
                    sceneChangeEvent.Fire(OutboundPackets.SceneChange, 0x01);
                }
            }
        }

        public void FlightStatusProvider()
        {
            if(FlightGlobals.ActiveVessel == null || TimeWarp.fetch == null)
            {
                return;
            }

            myFlightStatus.flightStatusFlags = 0;
            if (HighLogic.LoadedSceneIsFlight) myFlightStatus.flightStatusFlags += FlightStatusBits.isInFlight;
            if (FlightGlobals.ActiveVessel.isEVA) myFlightStatus.flightStatusFlags += FlightStatusBits.isEva;
            if (FlightGlobals.ActiveVessel.IsRecoverable) myFlightStatus.flightStatusFlags += FlightStatusBits.isRecoverable;
            if (TimeWarp.fetch.Mode == TimeWarp.Modes.LOW) myFlightStatus.flightStatusFlags += FlightStatusBits.isInAtmoTW;

            myFlightStatus.vesselSituation = (byte) FlightGlobals.ActiveVessel.situation;
            myFlightStatus.currentTWIndex = (byte) TimeWarp.fetch.current_rate_index;
            myFlightStatus.crewCapacity = (byte) Math.Min(Byte.MaxValue, FlightGlobals.ActiveVessel.GetCrewCapacity());
            myFlightStatus.crewCount = (byte) Math.Min(Byte.MaxValue, FlightGlobals.ActiveVessel.GetCrewCount());
            myFlightStatus.commNetSignalStrenghPercentage = (byte) Math.Round(100*FlightGlobals.ActiveVessel.connection.SignalStrength);

            if (flightStatusChannel != null) flightStatusChannel.Fire(OutboundPackets.FlightStatus, myFlightStatus);
        }
    }
}
