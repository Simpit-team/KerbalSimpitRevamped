using System;
using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitActionProvider : MonoBehaviour
{
    private EventData<byte, object> stageChannel;

    private volatile bool fireStage;
    private bool oldStageState;

    public void Start()
    {
        fireStage = false;
        oldStageState = false;

        stageChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived5");
        if (stageChannel != null) stageChannel.Add(stageCallback);
    }

    public void OnDestroy()
    {
        if (stageChannel != null) stageChannel.Remove(stageCallback);
    }

    public void Update()
    {
        if (fireStage)
        {
            StageManager.ActivateNextStage();
            fireStage = false;
        }
    }

    public void stageCallback(byte ID, object Data)
    {
        bool StageState = ((byte[])Data)[0] > 0;
        if (StageState != oldStageState)
        {
            if (StageState)
            {
                if (KerbalSimPit.KSPitConfig.Verbose) Debug.Log("KerbalSimPit: Staging!");
                fireStage = true;
            } else {
                if (KerbalSimPit.KSPitConfig.Verbose) Debug.Log("KerbalSimPit: Not staging");
            }
            oldStageState = StageState;
        }
    }
}
