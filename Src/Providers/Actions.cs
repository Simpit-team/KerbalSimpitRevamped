using System;
using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitActionProvider : MonoBehaviour
{
    private EventData<byte, object> AGActivateChannel, AGDeactivateChannel;

    // TODO: Only using a single byte buffer for each of these is
    // technically unsafe. It's not impossible that multiple controllers
    // will attempt to send new packets between each Update(), and only
    // the last one will be affected. But it is unlikely, which is why
    // I'm not addressing it now.
    private volatile byte activateBuffer, deactivateBuffer, toggleBuffer;

    public void Start()
    {
        activateBuffer = 0;
        deactivateBuffer = 0;
        toggleBuffer = 0;
        
        AGActivateChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived9");
        if (AGActivateChannel != null) AGActivateChannel.Add(actionActivateCallback);
        AGDeactivateChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived10");
        if (AGDeactivateChannel != null) AGDeactivateChannel.Add(actionDeactivateCallback);
    }

    public void OnDestroy()
    {
        //if (stageChannel != null) stageChannel.Remove(stageCallback);
        if (AGActivateChannel != null) AGActivateChannel.Remove(actionActivateCallback);
        if (AGDeactivateChannel != null) AGDeactivateChannel.Remove(actionDeactivateCallback);
    }

    public void Update()
    {
        Vessel av = FlightGlobals.ActiveVessel;
        if (activateBuffer > 0)
        {
            if ((activateBuffer & ActionGroupBits.StageBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating stage");
                av.ActionGroups.SetGroup(KSPActionGroup.Stage, true);
                StageManager.ActivateNextStage();
            }
            if ((activateBuffer & ActionGroupBits.GearBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating gear");
                av.ActionGroups.SetGroup(KSPActionGroup.Gear, true);
            }
            if ((activateBuffer & ActionGroupBits.LightBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating light");
                av.ActionGroups.SetGroup(KSPActionGroup.Light, true);
            }
            if ((activateBuffer & ActionGroupBits.RCSBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating RCS");
                av.ActionGroups.SetGroup(KSPActionGroup.RCS, true);
            }
            if ((activateBuffer & ActionGroupBits.SASBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating SAS");
                av.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            }
            if ((activateBuffer & ActionGroupBits.BrakesBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating brakes");
                av.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
            }
            if ((activateBuffer & ActionGroupBits.AbortBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating abort");
                av.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
            }
            activateBuffer = 0;
        }
        if (deactivateBuffer > 0)
        {
            if ((deactivateBuffer & ActionGroupBits.StageBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating stage");
                av.ActionGroups.SetGroup(KSPActionGroup.Stage, false);
            }
            if ((deactivateBuffer & ActionGroupBits.GearBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating gear");
                av.ActionGroups.SetGroup(KSPActionGroup.Gear, false);
            }
            if ((deactivateBuffer & ActionGroupBits.LightBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating light");
                av.ActionGroups.SetGroup(KSPActionGroup.Light, false);
            }
            if ((deactivateBuffer & ActionGroupBits.RCSBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating RCS");
                av.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
            }
            if ((deactivateBuffer & ActionGroupBits.SASBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating SAS");
                av.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
            }
            if ((deactivateBuffer & ActionGroupBits.BrakesBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating brakes");
                av.ActionGroups.SetGroup(KSPActionGroup.Brakes, false);
            }
            if ((deactivateBuffer & ActionGroupBits.AbortBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating abort");
                av.ActionGroups.SetGroup(KSPActionGroup.Abort, false);
            }
            deactivateBuffer = 0;
        }
        if (toggleBuffer > 0)
        {
            if ((toggleBuffer & ActionGroupBits.StageBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling stage");
                av.ActionGroups.ToggleGroup(KSPActionGroup.Stage);
                StageManager.ActivateNextStage();
            }
            if ((toggleBuffer & ActionGroupBits.GearBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling gear");
                av.ActionGroups.ToggleGroup(KSPActionGroup.Gear);
            }
            if ((toggleBuffer & ActionGroupBits.LightBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling light";
                av.ActionGroups.ToggleGroup(KSPActionGroup.Light);
            }
            if ((toggleBuffer & ActionGroupBits.RCSBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling RCS");
                av.ActionGroups.ToggleGroup(KSPActionGroup.RCS);
            }
            if ((toggleBuffer & ActionGroupBits.SASBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling SAS");
                av.ActionGroups.ToggleGroup(KSPActionGroup.SAS);
            }
            if ((toggleBuffer & ActionGroupBits.BrakesBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling brakes");
                av.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
            }
            if ((toggleBuffer & ActionGroupBits.AbortBit) != 0)
            {
                if (KerbalSimPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling abort");
                av.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
            }
            toggleBuffer = 0;
        }
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
}
