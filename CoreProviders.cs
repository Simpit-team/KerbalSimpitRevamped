using System;
using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitEchoProvider : MonoBehaviour
{
    private EventData<byte, object> echoRequestEvent;
    private EventData<byte, object> echoReplyEvent;

    public void Start()
    {
        echoRequestEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived1");
        if (echoRequestEvent != null) echoRequestEvent.Add(EchoRequestCallback);
        echoReplyEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived2");
        if (echoReplyEvent != null) echoReplyEvent.Add(EchoReplyCallback);
    }

    public void OnDestroy()
    {
        if (echoRequestEvent != null) echoRequestEvent.Remove(EchoRequestCallback);
        if (echoReplyEvent != null) echoReplyEvent.Remove(EchoReplyCallback);
    }

    public void EchoRequestCallback(byte ID, object Data)
    {
        if (KerbalSimPit.KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Echo request on port {0}. Replying.", ID));
        KerbalSimPit.SendToSerialPort(ID, 0x02, Data);
    }

    public void EchoReplyCallback(byte ID, object Data)
    {
        Debug.Log(String.Format("KerbalSimPit: Echo reply received on port {0}.", ID));
    }
}
