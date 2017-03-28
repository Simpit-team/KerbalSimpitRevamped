using System;
using System.Reflection;
using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitCAGProvider : MonoBehaviour
{
    private EventData<byte, object> enableChannel, disableChannel;
    private static bool AGXPresent;
    private static Type AGXExternal;
    
    private static KSPActionGroup[] ActionGroupIDs = new KSPActionGroup[] {
        KSPActionGroup.None,
        KSPActionGroup.Custom01,
        KSPActionGroup.Custom02,
        KSPActionGroup.Custom03,
        KSPActionGroup.Custom04,
        KSPActionGroup.Custom05,
        KSPActionGroup.Custom06,
        KSPActionGroup.Custom07,
        KSPActionGroup.Custom08,
        KSPActionGroup.Custom09,
        KSPActionGroup.Custom10
    };

    public void Start()
    {
        AGXPresent = AGXInstalled();
        if (KerbalSimPit.Config.Verbose) Debug.Log(String.Format("KerbalSimPit: ActionGroupsExtended installed: {0}", AGXPresent));

        enableChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived6");
        if (enableChannel != null) enableChannel.Add(enableCAGCallback);
        disableChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived7");
        if (disableChannel != null) disableChannel.Add(disableCAGCallback);
    }

    public void OnDestroy()
    {
        if (enableChannel != null) enableChannel.Remove(enableCAGCallback);
        if (disableChannel != null) disableChannel.Remove(disableCAGCallback);
    }

    public static bool AGXInstalled()
    {
        try
        {
            AGXExternal = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            return (bool)AGXExternal.InvokeMember("AGXInstalled",
                     BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                     null, null, null);
        } catch {
            return false;
        }
    }

    public static bool AGXActivateGroupDelayCheck(int group, bool forceDir)
    {
        if (AGXPresent)
        {
            return (bool)AGXExternal.InvokeMember("AGXActivateGroupDelayCheck",
                     BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                     null, null, new System.Object[] { group, forceDir});
        } else {
            return false;
        }
    }

    public void enableCAGCallback(byte ID, object Data)
    {
        byte[] groupIDs = (byte[])Data;
        int idx;
        for (int i=groupIDs.Length; i>=0; i--)
        {
            idx = (int)groupIDs[i];
            if (AGXPresent)
            {
                AGXActivateGroupDelayCheck(idx, true);
            } else {
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup
                    (ActionGroupIDs[idx], true);
            }
        }
    }

    public void disableCAGCallback(byte ID, object Data)
    {
        byte[] groupIDs = (byte[])Data;
        int idx;
        for (int i=groupIDs.Length; i>=0; i--)
        {
            idx = (int)groupIDs[i];
            if (AGXPresent)
            {
                AGXActivateGroupDelayCheck(idx, false);
            } else {
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup
                    (ActionGroupIDs[idx], false);
            }
        }
    }
}
