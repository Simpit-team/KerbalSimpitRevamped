using System;
using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitEchoProvider : MonoBehaviour, KerbalSimPitProvider
{
    // I don't think this is going to be used for echoes.
    public event EventHandler<KerbalSimPitDataEventArgs> SerialData;

    public void Start()
    {
        KerbalSimPit.AddFromDeviceHandler(0x01, EchoRequestHandler);
        KerbalSimPit.AddFromDeviceHandler(0x02, EchoReplyHandler);
    }

    public void EchoRequestHandler(object sender, KSPSerialPortEventArgs e)
    {
        Debug.Log("Got an echo request");
    }

    public void EchoReplyHandler(object sender, KSPSerialPortEventArgs e)
    {
        Debug.Log("Got an echo reply");
    }
}
