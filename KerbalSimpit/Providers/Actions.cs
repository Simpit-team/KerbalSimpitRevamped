using System;
using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitActionProvider : MonoBehaviour
    {
        // Inbound messages
        private EventData<byte, object> AGActivateChannel, AGDeactivateChannel,
            AGToggleChannel;

        // Outbound messages
        private EventData<byte, object> AGStateChannel;

        // TODO: Only using a single byte buffer for each of these is
        // technically unsafe. It's not impossible that multiple controllers
        // will attempt to send new packets between each Update(), and only
        // the last one will be affected. But it is unlikely, which is why
        // I'm not addressing it now.
        private volatile byte activateBuffer, deactivateBuffer,
            toggleBuffer, currentStateBuffer;

        // If set to true, the state should be sent at the next update even if no changes
        // are detected (for instance to initialise it after a new registration).
        private bool resendState = false;

        public void Start()
        {
            activateBuffer = 0;
            deactivateBuffer = 0;
            toggleBuffer = 0;
            currentStateBuffer = 0;

            AGActivateChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.ActionGroupActivate);
            if (AGActivateChannel != null) AGActivateChannel.Add(actionActivateCallback);
            AGDeactivateChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.ActionGroupDeactivate);
            if (AGDeactivateChannel != null) AGDeactivateChannel.Add(actionDeactivateCallback);
            AGToggleChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + +InboundPackets.ActionGroupToggle);
            if (AGToggleChannel != null) AGToggleChannel.Add(actionToggleCallback);

            AGStateChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.ActionGroups);
            GameEvents.FindEvent<EventData<byte, object>>("onSerialChannelForceSend" + OutboundPackets.ActionGroups).Add(resendActionGroup);
        }

        public void OnDestroy()
        {
            if (AGActivateChannel != null) AGActivateChannel.Remove(actionActivateCallback);
            if (AGDeactivateChannel != null) AGDeactivateChannel.Remove(actionDeactivateCallback);
            if (AGToggleChannel != null) AGToggleChannel.Remove(actionToggleCallback);
        }

        public void resendActionGroup(byte ID, object Data)
        {
            resendState = true;
        }

        public void Update()
        {
            Vessel av = FlightGlobals.ActiveVessel;
            if (activateBuffer > 0)
            {
                activateGroups(activateBuffer);
                activateBuffer = 0;
            }
            if (deactivateBuffer > 0)
            {
                deactivateGroups(deactivateBuffer);
                deactivateBuffer = 0;
            }
            if (toggleBuffer > 0)
            {
                toggleGroups(toggleBuffer);
                toggleBuffer = 0;
            }

            updateCurrentState();
        }

        public void actionActivateCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            activateBuffer = payload[0];
        }

        public void actionDeactivateCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            deactivateBuffer = payload[0];
        }

        public void actionToggleCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            toggleBuffer = payload[0];
        }

        private bool updateCurrentState()
        {
            byte newState = getGroups();
            if (newState != currentStateBuffer || resendState)
            {
                resendState = false;
                if (AGStateChannel != null) {
                    AGStateChannel.Fire(OutboundPackets.ActionGroups, newState);
                    currentStateBuffer = newState;
                }
                return true;
            } else {
                return false;
            }
        }

        private void activateGroups(byte groups)
        {
            if ((groups & ActionGroupBits.StageBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating stage");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, true);
                StageManager.ActivateNextStage();
            }
            if ((groups & ActionGroupBits.GearBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating gear");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, true);
            }
            if ((groups & ActionGroupBits.LightBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating light");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, true);
            }
            if ((groups & ActionGroupBits.RCSBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating RCS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, true);
            }
            if ((groups & ActionGroupBits.SASBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating SAS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            }
            if ((groups & ActionGroupBits.BrakesBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating brakes");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
            }
            if ((groups & ActionGroupBits.AbortBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Activating abort");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
            }
        }

        private void deactivateGroups(byte groups)
        {
            if ((groups & ActionGroupBits.StageBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating stage");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, false);
            }
            if ((groups & ActionGroupBits.GearBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating gear");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, false);
            }
            if ((groups & ActionGroupBits.LightBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating light");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, false);
            }
            if ((groups & ActionGroupBits.RCSBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating RCS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
            }
            if ((groups & ActionGroupBits.SASBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating SAS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
            }
            if ((groups & ActionGroupBits.BrakesBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating brakes");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, false);
            }
            if ((groups & ActionGroupBits.AbortBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Deactivating abort");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, false);
            }
        }

        private void toggleGroups(byte groups)
        {
            if ((groups & ActionGroupBits.StageBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling stage");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Stage);
                StageManager.ActivateNextStage();
            }
            if ((groups & ActionGroupBits.GearBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling gear");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Gear);
            }
            if ((groups & ActionGroupBits.LightBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling light");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Light);
            }
            if ((groups & ActionGroupBits.RCSBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling RCS");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.RCS);
            }
            if ((groups & ActionGroupBits.SASBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling SAS");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.SAS);
            }
            if ((groups & ActionGroupBits.BrakesBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling brakes");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
            }
            if ((groups & ActionGroupBits.AbortBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimpit: Toggling abort");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
            }
        }

        private byte getGroups()
        {
            if (FlightGlobals.ActiveVessel == null) return 0;

            byte groups = 0;
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Stage])
            {
                groups = (byte)(groups | ActionGroupBits.StageBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Gear])
            {
                groups = (byte)(groups | ActionGroupBits.GearBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Light])
            {
                groups = (byte)(groups | ActionGroupBits.LightBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS])
            {
                groups = (byte)(groups | ActionGroupBits.RCSBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.SAS])
            {
                groups = (byte)(groups | ActionGroupBits.SASBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Brakes])
            {
                groups = (byte)(groups | ActionGroupBits.BrakesBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Abort])
            {
                groups = (byte)(groups | ActionGroupBits.AbortBit);
            }
            return groups;
        }
    }
}
